using ArangoDBNetStandard.Transport.Http;
using ArangoDBNetStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AQLQueryRunner
{
    public class QueryRunner<T>
    {
        public const string DB_URL = @"http://localhost:8529/";
        public const string NEIGHBOR_NAME_PREFIX = "neighboursLevel";

        public static async Task<List<T>> QueryGraph(string verticesCollection,
            string graphCollection,
            int level,
            string[] allowedKeys,
            QueryFilter[] filters,
            string innerQueryOptions = ""
            )
        {
            var levelQuery = ConstructNeighbourLevelQuery(
                level,
                graphCollection,
               filters,
                innerQueryOptions);
            var query = $"""
                    FOR item IN {verticesCollection}
                    FILTER item._key IN [{FormatAllowedKeys(allowedKeys)}]
                    LET neighborId0 = item._id
                    {levelQuery}
                    LET itemWithNeighbors = MERGE(item, {GetResultShape("neighboursLevel1")})
                    RETURN {GetResultShape("itemWithNeighbors", "Result")}
                    """;


            using var systemDbTransport = HttpApiTransport.UsingNoAuth(new Uri(DB_URL));
            var adb = new ArangoDBClient(systemDbTransport);
            var response = await adb.Cursor.PostCursorAsync<T>(query);
            var result = response.Result.ToList();
            return result;
        }
        private static string GetResultShape(string resultProperty, string propertyName = "neighbors")
        {
            return "{ " + propertyName + ":  " + resultProperty + "}";
        }
        private static string FormatAllowedKeys(string[] allowedKeys)
        {
            var val = "";

            foreach (var item in allowedKeys)
            {
                val += $"\"{item}\",";
            }
            return val.Substring(0, val.Length - 1);
        }

        private static string ConstructNeighbourLevelQuery(int level, string graphName, QueryFilter[] filters, string innerQueryOptions = "")
        {
            var queries = new List<string>();
            if (string.IsNullOrEmpty(innerQueryOptions))
            {
                innerQueryOptions = """{ bfs: true, uniqueEdges: "path" }""";
            }

            for (var i = level; i > 0; i--)
            {
                var filter = GetFilterCondition(filters, i);
                var returnStatement = i == level ?
                    $"RETURN v{i}" :
                    $"""RETURN MERGE(v{i}, {GetResultShape(NEIGHBOR_NAME_PREFIX + (i + 1))})""";

                queries.Add($"""
                  LET {NEIGHBOR_NAME_PREFIX + i} = (
                            FOR v{i} IN 1..1 ANY neighborId{i - 1} GRAPH "{graphName}"
                            OPTIONS {innerQueryOptions}
                            {filter}
                            LET neighborId{i} = v{i}._id
                                PASTE_TEMPLATE
                            {returnStatement}
                        )
                """);
            }


            var compiledQuery = "";
            queries.ForEach(q =>
            {
                q = q.Replace("PASTE_TEMPLATE", compiledQuery);
                compiledQuery = q;
            });
            return compiledQuery;
        }

        private static string GetFilterCondition(QueryFilter[] filters, int level)
        {
            var parsedFilter = "";
            if (filters != null || filters.Count() > 0)
            {

                filters.ToList().ForEach(f =>
                {
                    parsedFilter += $""" {f.PreviousCondition.GetSymbol()} v{level}.{f.PropertyName} {f.Operation.GetSymbol()} "{f.CompareTo}" """;
                });
                parsedFilter = "FILTER" + parsedFilter;
            }
            return parsedFilter;
        }
    }

    public class QueryFilter
    {
        public Conditions PreviousCondition { get; set; } = Conditions.NONE;
        public string PropertyName { get; set; }
        public FilterOperators Operation { get; set; }
        public string CompareTo { get; set; }
    }
    public enum FilterOperators
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
    public enum Conditions
    {
        NONE,
        OR,
        AND
    }
    public static class FilterOperatorsExtensions
    {
        public static string GetSymbol(this FilterOperators val)
        {
            switch (val)
            {
                case FilterOperators.Equal:
                    return "==";
                case FilterOperators.NotEqual:
                    return "!=";
                case FilterOperators.GreaterThan:
                    return ">";
                case FilterOperators.LessThan:
                    return "<";
                case FilterOperators.GreaterThanOrEqual:
                    return ">=";
                case FilterOperators.LessThanOrEqual:
                    return "<=";
                default:
                    return "==";
            }
        }
    }
    public static class ConditionsExtensions
    {
        public static string GetSymbol(this Conditions val)
        {
            switch (val)
            {
                case Conditions.NONE:
                    return "";
                case Conditions.AND:
                    return "AND";
                case Conditions.OR:
                    return "OR";
                default:
                    return "";
            }
        }
    }
}
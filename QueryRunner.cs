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

        public static async Task<List<T>> QueryGraph(
            string graphCollection,
            string startVertexId,
            int level,

            QueryFilter[] filters,
            string innerQueryOptions = ""
            )
        {
            var levelQuery = ConstructNeighbourLevelQuery(
                graphCollection,
               filters,
                innerQueryOptions);
            var query = $"""
                    FOR v IN 1..{level}
                    OUTBOUND 
                    '{startVertexId}' 
                    GRAPH '{graphCollection}'
                    LET neighborId = v._id
                        {levelQuery}
                    LET itemWithNeighbors = MERGE(v, {GetResultShape("neighbours")})
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

        private static string ConstructNeighbourLevelQuery(string graphName, QueryFilter[] filters, string innerQueryOptions = "")
        {

            if (string.IsNullOrEmpty(innerQueryOptions))
            {
                innerQueryOptions = """{ bfs: true, uniqueEdges: "path" }""";
            }
            var vertexVariableName = "vertex";
            var filter = GetFilterCondition(filters, vertexVariableName);

            return $"""
                  LET neighbours = (
                            FOR {vertexVariableName} IN 1..1 ANY neighborId GRAPH "{graphName}"
                            OPTIONS {innerQueryOptions}
                            {filter}
                            RETURN {vertexVariableName}
                        )
                """;
        }

        private static string GetFilterCondition(QueryFilter[] filters, string vertexVariableName)
        {
            var parsedFilter = "";
            if (filters != null || filters.Count() > 0)
            {
                filters.ToList().ForEach(f =>
                {
                    parsedFilter += $""" {f.PreviousCondition.GetSymbol()} {vertexVariableName}.{f.PropertyName} {f.Operation.GetSymbol()} "{f.CompareTo}" """;
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
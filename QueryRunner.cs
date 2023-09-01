using ArangoDBNetStandard.Transport.Http;
using ArangoDBNetStandard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Reflection;

namespace AQLQueryRunner
{
    public class QueryRunner<T>
    {
        public const string DB_URL = @"http://localhost:8529/";
        public const string NEIGHBOR_NAME_PREFIX = "neighboursLevel";

        public static async Task<List<T[]>> QueryGraph(
            string graphCollection,
            string startNodeId,
            string endNodeId,
            QueryFilter[] filters,
            string innerQueryOptions = ""
            )
        {

            const string algoForTravesel = "K_SHORTEST_PATHS";

            var query = $"""
            FOR Paths 
            IN ANY {algoForTravesel} 
            '{startNodeId}' TO '{endNodeId}'
            GRAPH "{graphCollection}"
            RETURN DISTINCT
            Paths.vertices[*]
            """;

            using var systemDbTransport = HttpApiTransport.UsingNoAuth(new Uri(DB_URL));
            var adb = new ArangoDBClient(systemDbTransport);
            var response = await adb.Cursor.PostCursorAsync<T[]>(query);
            var result = response.Result.ToList();
            return result;
        }

        public static async Task<string> GetFormattedJson(List<T[]> paths)
        {
            var json = "[{";
            paths.ForEach(p =>
            {
                var pathJson = "";
                p.Reverse().ToList().ForEach(x =>
                {
                    var y = SingleNodeSerialization(x);
                    if (pathJson != "")
                    {
                        pathJson = $",{pathJson}";
                    }
                    y = y.Replace("PLACEHOLDER", pathJson);

                    pathJson = y;
                });
                json += pathJson + ",";
            });
            json = json.Substring(0, json.Length - 1);
            json += "}]";
            return json;
        }

        private static string SingleNodeSerialization(T Node)
        {
            var json = "";
            Type type = Node.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.Name == "_id")
                {
                    json += $"""
                        "{field.GetValue(Node)}":
                        """;
                    json += "{";
                }
                object value = field.GetValue(Node);
                json += $"""
                    "{field.Name}":"{value}",
                    """;
            }
            json = json.Substring(0, json.Length - 1);
            json += " PLACEHOLDER }";
            return json;
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
        public required string PropertyName { get; set; }
        public FilterOperators Operation { get; set; }
        public required string CompareTo { get; set; }
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
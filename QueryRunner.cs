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
            string[] allowedKeys)
        {
            var levelQuery = ConstructNeighbourLevelQuery(level, graphCollection);
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

        private static string ConstructNeighbourLevelQuery(int level, string graphName)
        {
            var queries = new List<string>();
            var innerQueryOptions = """{ bfs: true, uniqueEdges: "path" }""";

            for (var i = level; i > 0; i--)
            {
                var filter = $"""
                 v{i}.type != "type" OR v{i}.field != ""
                 """;

                if (i == level)
                {
                    queries.Add($"""
                  LET {NEIGHBOR_NAME_PREFIX + i} = (
                            FOR v{i} IN 1..1 ANY neighborId{i - 1} GRAPH "{graphName}"
                            OPTIONS {innerQueryOptions}
                            FILTER {filter}
                            LET neighborId{i} = v{i}._id
                                PASTE_TEMPLATE
                            RETURN v{i}
                        )
                """);
                }
                else
                {
                    queries.Add($"""
                  LET {NEIGHBOR_NAME_PREFIX + i} = (
                            FOR v{i} IN 1..1 ANY neighborId{i - 1} GRAPH "{graphName}"
                            OPTIONS {innerQueryOptions}
                            FILTER {filter}
                            LET neighborId{i} = v{i}._id
                                PASTE_TEMPLATE
                            RETURN MERGE(v{i}, {GetResultShape(NEIGHBOR_NAME_PREFIX + (i + 1))})
                        )
                """);
                }


            }


            var compiledQuery = "";
            queries.ForEach(q =>
            {
                q = q.Replace("PASTE_TEMPLATE", compiledQuery);
                compiledQuery = q;
            });
            return compiledQuery;
        }
    }
}
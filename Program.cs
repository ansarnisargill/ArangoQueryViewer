using ArangoDBNetStandard;
using ArangoDBNetStandard.Transport.Http;
using System;
using System.Text.Json;

namespace AQLQueryRunner
{
    class ApplicationMain
    {
        static void Main(string[] args)
        {
            using var systemDbTransport = HttpApiTransport.UsingNoAuth(new Uri(@"http://localhost:8529/"));
            {
                var adb = new ArangoDBClient(systemDbTransport);
                var result = QueryRunner<Node>.QueryGraph(adb, "mps_graph", "mps_verts/A", "mps_verts/B").Result;
                var output = QueryRunner<Node>.GetStepWisePaths(result).Result;
                var json = JsonSerializer.Serialize(output);
                Console.Write(json);
            }

        }
    }

    public class Node : NodeParent
    {
        public string _key { get; set; }
        public string _rev { get; set; }
    }
}
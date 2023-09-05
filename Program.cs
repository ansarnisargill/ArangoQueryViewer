using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AQLQueryRunner
{
    class ApplicationMain
    {
        static void Main(string[] args)
        {
            var result = QueryRunner<Node>.QueryGraph(
                 "mps_graph",
                 "mps_verts/A",
                 "mps_verts/B",
                 new QueryFilter[] {
                    new QueryFilter() {
                        PreviousCondition= Conditions.NONE ,
                        Operation = FilterOperators.NotEqual,
                        PropertyName = "type",
                        CompareTo="type"
                    },
                    new QueryFilter() {
                        PreviousCondition= Conditions.OR ,
                        Operation = FilterOperators.NotEqual,
                        PropertyName = "field",
                        CompareTo=""
                    }
                 }
                 ).Result;
            var output = QueryRunner<Node>.GetStepWisePaths(result).Result;
            var json = JsonSerializer.Serialize(output);
            Console.Write(json);
        }
    }

    public class Node : NodeParent
    {
        public string _key { get; set; }
        public string _rev { get; set; }
    }
}
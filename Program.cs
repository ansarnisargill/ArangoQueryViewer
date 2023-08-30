using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AQLQueryRunner
{
    class ApplicationMain
    {
        static void Main(string[] args)
        {
            var result = QueryRunner<Root>.QueryGraph(
                 "mps_graph",
                 "mps_verts/A",
                 "mps_verts/F",
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
            Console.Write(JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }

    public class Neighbor
    {
        public string _id { get; set; }
        public string _key { get; set; }
        public string _rev { get; set; }
        public List<Neighbor> neighbors { get; set; }
    }

    public class Root
    {
        public Neighbor Result { get; set; }
    }
}
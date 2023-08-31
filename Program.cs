using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AQLQueryRunner
{
    class ApplicationMain
    {
        static void Main(string[] args)
        {
            var result = QueryRunner<Node>.QueryGraph(
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
            var json = QueryRunner<Node>.GetFormattedJson(result).Result;
            Console.Write(json);
        }
    }

    public class Node
    {
        public string _id;
        public string _key;
        public string _rev;
    }
}
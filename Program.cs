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

    public class Node : NodeParent
    {
        public string _key;
        public string _rev;
    }
    public class NodeParent
    {
        public string _id;

        [JsonIgnore]
        public string parent_id = "";

        public override bool Equals(object obj)
        {
            NodeParent item = (NodeParent)obj;
            return _id == item._id && parent_id == item.parent_id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
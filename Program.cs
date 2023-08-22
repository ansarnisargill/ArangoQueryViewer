using System.Collections.Generic;

namespace AQLQueryRunner
{
    class ApplicationMain
    {
        static void Main(string[] args)
        {
            QueryRunner<Root>.QueryGraph(
                "mps_verts",
                "mps_graph",
                3,
                new string[] { "A", "B" },
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
                ).Wait();
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
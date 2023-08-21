using System.Collections.Generic;

namespace AQLQueryRunner
{
    class ApplicationMain
    {
        static void Main(string[] args)
        {
            QueryRunner<Root>.QueryGraph("mps_verts", "mps_graph", 3, new string[] { "A", "B" }).Wait();
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
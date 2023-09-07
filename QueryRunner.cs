using ArangoDBNetStandard;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace AQLQueryRunner
{
    public class QueryRunner<T> where T : NodeParent
    {
        public static string Algorithm = "K_SHORTEST_PATHS";

        public static async Task<List<T[]>> QueryGraph(
            ArangoDBClient adb,
            string graphCollection,
            string startNodeId,
            string endNodeId
            )
        {
            var query = $"""
            FOR Paths 
            IN ANY {Algorithm} 
            '{startNodeId}' TO '{endNodeId}'
            GRAPH "{graphCollection}"
            RETURN DISTINCT
            Paths.vertices[*]
            """;

            var response = await adb.Cursor.PostCursorAsync<T[]>(query);
            var result = response.Result.ToList();
            return result;
        }




        public static async Task<T> GetStepWisePaths(List<T[]> paths)
        {
            T obj = default;

            var largestDepth = 1;
            paths.ForEach(p =>
            {
                if (largestDepth < p.Length)
                {
                    largestDepth = p.Length;
                }
            });

            for (var i = 0; i < largestDepth; i++)
            {
                List<T> itemsAtCurrentIndex = new List<T> { };
                paths.ForEach(p =>
                {
                    try
                    {
                        var obj = p[i];
                        if (i != 0)
                        {
                            obj.parent_id = p[i - 1]._id;
                        }

                        if (obj == null)
                        {
                            return;
                        }

                        if (!itemsAtCurrentIndex.Contains(obj))
                        {
                            itemsAtCurrentIndex.Add(obj);
                        }
                    }
                    catch { }
                });

                itemsAtCurrentIndex.GroupBy(x => x.parent_id).ToList().ForEach(item =>
                {
                    var innerItems = item.ToList();
                    if (i == 0)
                    {
                        obj = innerItems.First();
                    }
                    else
                    {
                        obj.AppendToDeepest(innerItems.ToArray(), item.Key);
                    }

                });
            }


            return obj;
        }
    }

    public class NodeParent
    {
        public string _id { get; set; }

        [JsonIgnore]
        public string parent_id = "";

        public NodeParent[] next { get; set; }


        public override bool Equals(object obj)
        {
            NodeParent item = (NodeParent)obj;
            return _id == item._id && parent_id == item.parent_id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
        public void AppendToDeepest(NodeParent[] nodes, string parentId)
        {
            if (this.next == null || this.next.Length == 0)
            {
                if (_id == parentId)
                {
                    this.next = nodes;
                }
                return;
            }

            foreach (NodeParent node in this.next)
            {
                node.AppendToDeepest(nodes, parentId);
            }
        }
    }
}
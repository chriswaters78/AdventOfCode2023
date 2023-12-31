using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2023_25
{
    public static class DistinctRoutes
    {
        /// <summary>
        /// For pairs of nodes, starting from the seed node, we test if we can make 4 distinct routes
        /// between them. If so they are part of the same set, if not they are in different sets
        /// We grow each set and merge nodes between them. Ultimately we end up with a set that is one side of a 3-cut
        /// Note this method relies on their being only one possible 3 cut as we grow both sets at once
        /// and so if there is more than one possible cut we will be merging nodes that shouldn't be merged
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static (int minCut, List<int> partition) MinimumCut(Dictionary<int, List<int>> graph)
        {
            Dictionary<int, Dictionary<int, int>> g = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(key => key, _ => 1));

            //partition the graph into two sets based on there being a single minimum cut of 3
            (var set1, var set2) = (new HashSet<int>(), new HashSet<int>());
            int set2First = -1;
            var rand = new Random();
            //choose our first node randomly, could just always choose first
            int key1 = g.Keys.ToList()[rand.Next(g.Count)];
            set1.Add(key1);
            while (g.Count > 2)
            {
                int key2 = g[key1].Keys.Where(key => !set1.Contains(key) && !set2.Contains(key)).FirstOrDefault(-1);
                //we are done early if we can't reach any more nodes in set 1
                //set 2 may be incomplete incomplete
                if (key2 == -1)
                    break;

                var without = new Dictionary<Edge, int>();
                List<Edge> path = null;
                for (int count = 0; count < 4; count++)
                {
                    path = shortestPathExcludingEdges(g, key1, key2, without);
                    if (path == null)
                        break;

                    foreach (var edge in path)
                    {
                        if (!without.ContainsKey(edge))
                            without[edge] = 0;

                        without[edge]++;
                    }
                }
                if (path != null)
                {
                    _2023_25.Graph.ContractEdgeNoHistory(g, new Edge(key1, key2));
                    set1.Add(key2);
                }
                else
                {
                    if (set2First == -1)
                    {
                        set2First = key2;
                        set2.Add(key2);
                    }
                    else
                    {
                        _2023_25.Graph.ContractEdgeNoHistory(g, new Edge(set2First, key2));
                        set2.Add(key2);
                    }
                }
            }
            //min cut is the number of outgoing edges from the set we have found
            var minCut = g[key1].Sum(tp => tp.Value);
            var partition = set1.ToList();

            return (minCut, partition);
        }

        static List<Edge> shortestPathExcludingEdges(Dictionary<int, Dictionary<int, int>> graph, int start, int end, Dictionary<Edge, int> without)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<(int, List<Edge>)>();
            visited.Add(start);
            queue.Enqueue((start, new List<Edge>()));
            while (queue.Any())
            {
                (var curr, var route) = queue.Dequeue();
                if (curr == end)
                    return route;

                foreach ((var edge, var edgeCount) in graph[curr])
                {
                    var newEdge = curr.CompareTo(edge) == -1 ? new Edge(curr, edge) : new Edge(edge, curr);
                    if (without.ContainsKey(newEdge) && without[newEdge] >= edgeCount)
                        continue;

                    if (visited.Contains(edge))
                        continue;

                    visited.Add(edge);
                    var newRoute = route.ToList();
                    newRoute.Add(newEdge);
                    queue.Enqueue((edge, newRoute));
                }
            }
            return null;
        }
    }
}
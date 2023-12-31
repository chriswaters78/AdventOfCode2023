using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2023_25
{
    public static class CountCrossings
    {
        static Random rand = new Random();

        /// <summary>
        /// We repeatedly pick two random vertices and find the shortest path between them via BFS.
        /// Then pick the top k most travelled edges and remove these from the graph and
        /// check if we have succesfully made a k cut. If so return it, if not we continue
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="noCrossings">How many crossings to collect statistics on per attempt</param>
        /// <param name="k">Stop when we find a k-cut</param>
        /// <returns>A k-cut and one of the partitions</returns>
        public static (int minCut, List<int>) MinimumCut(Dictionary<int, List<int>> graph, int noCrossings, int k)
        {
            HashSet<int> canReach = null;
            do
            {
                var crossingCounts = new Dictionary<(int from, int to), int>();
                for (int i = 0; i < noCrossings; i++)
                {
                    var v1 = graph.Keys.ToArray()[rand.Next(graph.Count)];
                    var v2 = graph.Keys.Where(key => key != v1).ToArray()[rand.Next(graph.Count - 1)];

                    var path = Graph.ShortestPath(graph, v1, v2);
                    for (int p = 1; p < path.Count; p++)
                    {
                        var key = path[p - 1].CompareTo(path[p]) == -1 ? (path[p - 1], path[p]) : (path[p], path[p - 1]);
                        if (!crossingCounts.ContainsKey(key))
                            crossingCounts[key] = 0;

                        crossingCounts[key]++;
                    }
                }

                var topK = crossingCounts.OrderByDescending(kvp => kvp.Value).Take(k).ToList();

                //remove the 3 edges that we are guessing make the min cut
                var g2 = graph.ToDictionary(KeyValuePair => KeyValuePair.Key, KeyValuePair => KeyValuePair.Value.ToList());
                for (int i = 0; i < k; i++)
                {
                    g2[topK[i].Key.from].Remove(topK[i].Key.to);
                    g2[topK[i].Key.to].Remove(topK[i].Key.from);
                }

                canReach = Graph.Reachable(g2, graph.Keys.First());
            } while (canReach.Count == graph.Count);

            return (k, canReach.ToList());
        }
    }
}

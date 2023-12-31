using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2023_25
{
    public static class Find4ConnectedSet
    {
        static Random rand = new();
        
        /// <summary>
        /// Pick a random starting node, and then for other random nodes, find a node that
        /// is not connected by 4 independent routes. Remove the edges for all the routes you did find
        /// And then all the nodes connected to our starting node are part of the cut
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        public static (int minCut, List<int> partition) MinimumCut(Dictionary<int, List<int>> graph)
        {
            var g = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
            int key1 = g.Keys.ToList()[rand.Next(g.Count)];
            while (g.Count > 2)
            {
                int key2 = g.Keys.Where(key => key != key1).ToArray()[rand.Next(g.Keys.Count - 1)];

                var without = new Dictionary<Edge, int>();
                List<List<int>> paths = new List<List<int>>();
                for (int count = 0; count < 4; count++)
                {
                    var path = shortestPathExcludingEdges(g, key1, key2, without);
                    if (path == null)
                        break;
        
                    for (int p = 1; p < path.Count; p++)
                    {
                        var pathKey = path[p] < path[p - 1] ? new Edge(path[p], path[p - 1]) : new Edge(path[p - 1], path[p]);
                        if (!without.ContainsKey(pathKey))
                            without[pathKey] = 0;

                        without[pathKey]++;
                    }
                    paths.Add(path);
                }

                if (paths.Count < 4)
                {
                    //we know key 1 and key 2 are in seperate segments if we perform a 3 cut
                    //So perform this 3 cut by removing all the paths edges for whatever routes we do have
                    //and counting the number of vertices still connected to key1
                    foreach (var path in paths)
                    {
                        for (int i = 1; i < path.Count; i++)
                        {
                            g[path[i - 1]].Remove(path[i]);
                            g[path[i]].Remove(path[i-1]);
                        }
                    }

                    var reachable = _2023_25.Graph.Reachable(g, key1);
                    return (3, reachable.ToList());
                }
            }

            throw new();
        }

        static List<int> shortestPathExcludingEdges(Dictionary<int, List<int>> graph, int start, int end, Dictionary<Edge, int> without)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<(int, List<int>)>();
            visited.Add(start);
            queue.Enqueue((start, [start]));
            while (queue.Any())
            {
                (var curr, var route) = queue.Dequeue();
                if (curr == end)
                    return route;

                foreach (var edge in graph[curr])
                {
                    var newEdge = curr < edge ? new Edge(curr, edge) : new Edge(edge, curr);
                    if (without.ContainsKey(newEdge))
                        continue;

                    if (visited.Contains(edge))
                        continue;

                    visited.Add(edge);
                    var newRoute = route.ToList();
                    newRoute.Add(edge);
                    queue.Enqueue((edge, newRoute));
                }
            }
            return null;
        }
    }
}
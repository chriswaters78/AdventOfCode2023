using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2023_25
{
    public static class Graph
    {
        /// <summary>
        /// Contract an edge in a weighted graph. We merge into named nodes and keep the merge history per node to avoid excessively long keys
        /// </summary>
        /// <param name="graph">Graph to operate on. This will be modified.</param>
        /// <param name="v1">The vertex that will survive and have its merge history updated</param>
        /// <param name="v2">The vertex that will merge into v1 and then be removed from the graph</param>
        public static void ContractEdge(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, int v1, int v2)
        {
            //we merge everything into v1 and keep that key
            //and record any merges in the merges list for that node
            //this is more efficient that using long merged strings as these will be stored multiple times as edges

            //remove the edge between v1 and v2
            //and record the merge history
            graph[v1].edges.Remove(v2);
            graph[v2].edges.Remove(v1);
            graph[v1].merges.AddRange(graph[v2].merges);

            //for every edge that did exist between v2 and v3
            foreach ((var v3, var count) in graph[v2].edges)
            {
                if (graph[v1].edges.ContainsKey(v3))
                {
                    graph[v1].edges[v3] += count;
                    graph[v3].edges[v1] += count;
                }
                else
                {
                    graph[v1].edges[v3] = count;
                    graph[v3].edges[v1] = count;
                }
                graph[v3].edges.Remove(v2);
            }

            graph.Remove(v2);
        }

        /// <summary>
        /// Same as above but we don't retain the merge history so slightly faster
        /// </summary>
        public static void ContractEdgeNoHistory(Dictionary<int, Dictionary<int, int>> graph, Edge edge)
        {
            var v1 = edge.from;
            var v2 = edge.to;

            graph[v1].Remove(v2);
            graph[v2].Remove(v1);

            foreach ((var v3, var count) in graph[v2])
            {
                if (graph[v1].ContainsKey(v3))
                {
                    graph[v1][v3] += count;
                    graph[v3][v1] += count;
                }
                else
                {
                    graph[v1][v3] = count;
                    graph[v3][v1] = count;
                }
                graph[v3].Remove(v2);
            }

            graph.Remove(v2);
        }

        /// <summary>
        /// All vertexes reachable in the unweighted graph from the starting point
        /// </summary>
        /// <param name="graph">An unweighted graph</param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static HashSet<int> Reachable(Dictionary<int, List<int>> graph, int start)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            visited.Add(start);
            queue.Enqueue(start);
            while (queue.Any())
            {
                var curr = queue.Dequeue();
                foreach (var edge in graph[curr])
                {
                    if (visited.Add(edge))
                        queue.Enqueue(edge);
                }
            }
            return visited;
        }

        /// <summary>
        /// The shortest path between two vertexes in an unweighted graph.
        /// </summary>
        /// <param name="graph">An unweighted graph</param>
        /// <param name="start">The start of the path</param>
        /// <param name="end"></param>
        /// <returns>The vertexes visited or null</returns>
        public static List<int> ShortestPath(Dictionary<int, List<int>> graph, int start, int end)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<(int, List<int>)>();
            visited.Add(start);
            queue.Enqueue((start, [start]));
            while (queue.Any())
            {
                (var curr, var route) = queue.Dequeue();
                if (curr == end)
                {
                    return route;
                }

                foreach (var edge in graph[curr])
                {
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

        public static Dictionary<int, List<int>> GenerateRandomHyperbolicGraph(int N, double alpha, double R)
        {
            Random rand = new Random();
            var graph = new Dictionary<(int, double r, double theta), HashSet<int>>();
            for (int n = 0; n < N; n++)
            {
                var theta = 2 * Math.PI * rand.NextDouble();
                var r = alpha * (Math.Acosh(1 + (Math.Acosh(alpha * R) - 1) * rand.NextDouble()));
                graph.Add((n, r, theta), new HashSet<int>());
            }

            foreach (var n1 in graph.Keys)
            {
                foreach (var n2 in graph.Keys)
                {
                    if (n1.Item1 == n2.Item1)
                        continue;

                    var d12y = n1.r * Math.Sin(n1.theta) - n2.r * Math.Sin(n2.theta);
                    var d12x = n1.r * Math.Cos(n1.theta) - n2.r * Math.Cos(n2.theta);
                    var d12 = Math.Sqrt(d12y*d12y + d12x*d12x);
                    if (d12 < R * rand.NextDouble())
                    {
                        graph[n1].Add(n2.Item1);
                        graph[n2].Add(n1.Item1);
                    }
                }
            }

            return graph.ToDictionary(kvp => kvp.Key.Item1, kvp => kvp.Value.ToList());
        }
    }
}

public readonly record struct Edge(int from, int to);
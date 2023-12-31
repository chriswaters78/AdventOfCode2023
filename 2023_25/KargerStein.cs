using QuikGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2023_25
{
    public static class KargerStein
    {
        static Random rand = new Random();
        
        public static (int minCut, List<int> partition) MinimumCut(ReadOnlyDictionary<int, List<int>> graph, int FINDCUT, bool useRecursive, double reductionFactor, int stopAt)
        {
            int bestCut = int.MaxValue;
            List<int> bestPartition = null;
            while (bestCut != FINDCUT)
            {
                Dictionary<int, (List<int> merges, Dictionary<int,int> edges)> g = graph.ToDictionary(kvp => kvp.Key, kvp => (new List<int>() { kvp.Key }, kvp.Value.ToDictionary(str => str, _ => 1)));
                if (useRecursive)
                {
                    g = recursiveContractKargers(g, reductionFactor, stopAt);
                }
                else
                {
                    contractKargers(g, 2);
                }

                var minCut = g.Values.First().edges.Sum(kvp => kvp.Value);
                if (minCut < bestCut)
                {
                    bestCut = minCut;
                    bestPartition = g.First().Value.merges;
                }
            }
            return (bestCut, bestPartition);
        }

        static Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> recursiveContractKargers(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, double reductionFactor, int stopAt)
        {
            int N = graph.Count;
            if (N <= stopAt)
            {
                //var g = graph.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.merges.ToList(), kvp.Value.edges.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)));
                contractKargers(graph, 2);
                return graph;
            }
            else
            {
                //seems sensitive to this, and in particular is quite slow
                //with the recommended factor of 1/sqrt(2), 2.1 seems to work better
                var limit = (int)(N / reductionFactor);

                //we can modify the original graph
                var g2 = graph.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.merges.ToList(), kvp.Value.edges.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)));

                contractKargers(graph, limit);
                var r1 = recursiveContractKargers(graph, reductionFactor, stopAt);

                contractKargers(g2, limit);
                var r2 = recursiveContractKargers(g2, reductionFactor, stopAt);

                return r1.Values.First().edges.Sum(kvp => kvp.Value) < r2.Values.First().edges.Sum(kvp => kvp.Value) ? r1 : r2;
            }
        }
        static void contractKargers(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, int k)
        {
            while (graph.Count > k)
            {
                (var vertex, var edge) = randomSelectKragers(graph);
                _2023_25.Graph.ContractEdge(graph, vertex, edge);
            }
        }

        static (int vertex, int edge) randomSelectKragers(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph)
        {
            //find weights for all keys
            var Ds = new int[graph.Count, 2];
            int acc = 0;
            int i = 0;
            foreach (var kvp in graph)
            {
                acc += kvp.Value.edges.Count;
                Ds[i, 0] = kvp.Key;
                Ds[i, 1] = acc;
                i++;
            }
            var randomVertex = rand.Next(acc);
            int chosenVertex = int.MinValue;
            for (int d = 0; d < Ds.Length; d++)
            {
                if (randomVertex < Ds[d, 1])
                {
                    chosenVertex = Ds[d, 0];
                    break;
                }
            }
            var Es = new int[graph[chosenVertex].edges.Count, 2];
            acc = 0;
            i = 0;
            foreach (var kvp in graph[chosenVertex].edges)
            {
                acc += kvp.Value;
                Es[i, 0] = kvp.Key;
                Es[i, 1] = acc;
                i++;
            }
            var randomEdge = rand.Next(acc);
            int chosenEdge = int.MinValue;
            for (int e = 0; e < Es.Length; e++)
            {
                if (randomEdge < Es[e, 1])
                {
                    chosenEdge = Es[e, 0];
                    break;
                }
            }

            return (chosenVertex, chosenEdge);
        }
    }
}

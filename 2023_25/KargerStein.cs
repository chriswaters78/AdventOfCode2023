using QuikGraph;
using QuikGraph.Collections;
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
            while (bestCut > FINDCUT)
            {
                var merges = new ForestDisjointSet<int>();

                var g = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(str => str, _ => 1));
                foreach ((var vertex, var edges) in g)
                {
                    merges.MakeSet(vertex);
                }

                (g, merges) = useRecursive ? recursiveContractKargers(g, merges, reductionFactor, stopAt)
                            : contractKargers(g, merges, 2);

                var minCut = g.Values.First().Sum(kvp => kvp.Value);
                if (minCut < bestCut)
                {
                    bestCut = minCut;
                    bestPartition = graph.Keys.Where(key => merges.AreInSameSet(key, g.First().Key)).ToList();
                }
            }
            return (bestCut, bestPartition);
        }

        static (Dictionary<int, Dictionary<int, int>>, ForestDisjointSet<int>) recursiveContractKargers(Dictionary<int, Dictionary<int, int>> graph, ForestDisjointSet<int> merges, double reductionFactor, int stopAt)
        {
            int N = graph.Count;
            if (N <= stopAt)
            {
                return contractKargers(graph, merges, 2);
            }
            else
            {
                //seems sensitive to this, and in particular is quite slow
                //with the recommended factor of 1/sqrt(2), 2.1 seems to work better
                var limit = (int)(N / reductionFactor);

                //we can modify the original graph
                var originalKeys = new List<int>();
                var set1 = new ForestDisjointSet<int>();
                var set2 = new ForestDisjointSet<int>();

                var g2 = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value));
                foreach (var key in g2.Keys)
                {
                    set1.MakeSet(key);
                    set2.MakeSet(key);
                    originalKeys.Add(key);
                }

                contractKargers(graph, set1, limit);
                var r1 = recursiveContractKargers(graph, set1, reductionFactor, stopAt);

                contractKargers(g2, set2, limit);
                var r2 = recursiveContractKargers(g2, set2, reductionFactor, stopAt);

                (var returnGraph, var returnSet) = r1.Item1.First().Value.Sum(kvp => kvp.Value) < r2.Item1.First().Value.Sum(kvp => kvp.Value) ? r1 : r2;
                foreach (var key in originalKeys)
                {
                    merges.Union(key, returnSet.FindSet(key));
                }

                return (returnGraph, merges);
            }
        }
        static (Dictionary<int, Dictionary<int, int>> graph, ForestDisjointSet<int> merges) contractKargers(Dictionary<int, Dictionary<int, int>> graph, ForestDisjointSet<int> merges, int k)
        {
            while (graph.Count > k)
            {
                (var vertex, var edge) = randomSelectKragers(graph);
                //can get an error if contract the only edge between two nodes
                _2023_25.Graph.ContractEdgeNoHistory(graph, new Edge(vertex,edge));
                merges.Union(vertex, edge);
            }
            return (graph, merges);
        }

        static (int vertex, int edge) randomSelectKragers(Dictionary<int, Dictionary<int, int>> graph)
        {
            //find weights for all keys
            var Ds = new int[graph.Count, 2];
            int acc = 0;
            int i = 0;
            foreach (var kvp in graph)
            {
                acc += kvp.Value.Count;
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
            var Es = new int[graph[chosenVertex].Count, 2];
            acc = 0;
            i = 0;
            foreach (var kvp in graph[chosenVertex])
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

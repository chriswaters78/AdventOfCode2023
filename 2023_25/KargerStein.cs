using QuikGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeLib;

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
            var tree = new TreeLib.AVLTreeMultiRankList<(int from, int to)>();
            foreach ((int from, int to, int count) in graph.SelectMany(
                vertex => vertex.Value.edges.Select(edge => (vertex.Key, edge.Key, edge.Value))))
            {
                tree.Add((from, to), count);
            }

            while (graph.Count > k)
            {
                //this is O(V+(E/V)) to select an edge as we have to go through all nodes
                //and then the edges for a node each time
                //can we maintain the weights better?
                //Ds = cumulative probability of selecting that node (sum of all edge weights for each node)
                //Es = cumulative probability of selecting an edge (sum of edge weights within node)
                //we remove edges connecting the two nodes, all other nodes retain their edge weights


                (var v1, var v2) = randomSelectKragersOST(tree, graph);

                //remove all existence of the edge in the tree
                tree.Remove((v1, v2));
                tree.Remove((v2, v1));

                //now we need to maintain our weight tree
                foreach ((var v3, var v3Weight) in graph[v2].edges)
                {
                    if (v3 == v1)
                        //removed already
                        continue;

                    //we remove from tree and re-add
                    //O(ln n) both
                    tree.Remove((v2, v3));
                    tree.Remove((v3, v2));

                    //and now we add edges from 
                    var newEdge = (v3, v1);
                    if (tree.ContainsKey(newEdge))
                    {
                        tree.Get(newEdge, out var _, out int _, out int rankCount);
                        tree.Set(newEdge, v3Weight + rankCount);
                    }
                    else
                    {
                        tree.Add(newEdge, v3Weight);
                    }

                    newEdge = (v1, v3);
                    if (tree.ContainsKey(newEdge))
                    {
                        tree.Get(newEdge, out var _, out int _, out int rankCount);
                        tree.Set(newEdge, v3Weight + rankCount);
                    }
                    else
                    {
                        tree.Add(newEdge, v3Weight);
                    }
                }

                _2023_25.Graph.ContractEdge(graph, v1, v2);
            }
        }
        static (int vertex, int edge) randomSelectKragersOST(TreeLib.AVLTreeMultiRankList<(int from, int to)> tree, Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph)
        {
            var edgeRank = rand.Next((int) tree.RankCount);
            //huh tree doesn't actually support retrieving the i'th element
            var edge = tree.GetKeyByRank(edgeRank);
            
            return edge;
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

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
                var tree = new TreeLib.AVLTreeMultiRankMap<int, TreeLib.AVLTreeMultiRankList<int>>();
                foreach ((var vertex, var edges) in graph)
                {
                    var edgeTree = new TreeLib.AVLTreeMultiRankList<int>();
                    int edgeTotal = 0;
                    foreach (var edge in edges)
                    {
                        edgeTree.Add(edge, 1);
                        edgeTotal += 1;
                    }
                    tree.Add(vertex, edgeTree, edgeTotal);
                }
                //Dictionary<int, (List<int> merges, Dictionary<int,int> edges)> g = graph.ToDictionary(kvp => kvp.Key, kvp => (new List<int>() { kvp.Key }, kvp.Value.ToDictionary(str => str, _ => 1)));
                if (useRecursive)
                {
                    tree = recursiveContractKargers(tree, reductionFactor, stopAt);
                }
                else
                {
                    contractKargers(tree, 2);
                }

                var minCut = tree.First().Count;
                if (minCut < bestCut)
                {
                    bestCut = minCut;
                    bestPartition = [tree.First().Key];
                }
            }
            return (bestCut, bestPartition);
        }

        static TreeLib.AVLTreeMultiRankMap<int, TreeLib.AVLTreeMultiRankList<int>> recursiveContractKargers(TreeLib.AVLTreeMultiRankMap<int, TreeLib.AVLTreeMultiRankList<int>> tree, double reductionFactor, int stopAt)
        {
            long N = tree.LongCount;
            if (N <= stopAt)
            {
                //var g = graph.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.merges.ToList(), kvp.Value.edges.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)));
                contractKargers(tree, 2);
                return tree;
            }
            else
            {
                //seems sensitive to this, and in particular is quite slow
                //with the recommended factor of 1/sqrt(2), 2.1 seems to work better
                var limit = (int)(N / reductionFactor);

                //we can modify the original graph
                var g2 = new AVLTreeMultiRankMap<int, AVLTreeMultiRankList<int>>(tree);
                foreach (var g2Tree in g2.ToArray())
                {
                    //g2.Get(g2Tree.Key, out var g2TreeFull, out var g2TreeFullRank, out var g2TreeFullRankCount);
                    g2.Set(g2Tree.Key, new TreeLib.AVLTreeMultiRankList<int>(g2Tree.Value), g2Tree.Count);
                }
                contractKargers(tree, limit);
                var r1 = recursiveContractKargers(tree, reductionFactor, stopAt);

                contractKargers(g2, limit);
                var r2 = recursiveContractKargers(g2, reductionFactor, stopAt);

                return r1.First().Count < r2.First().Count ? r1 : r2;
            }
        }
        static void contractKargers(TreeLib.AVLTreeMultiRankMap<int, TreeLib.AVLTreeMultiRankList<int>> tree, int k)
        {
            //var tree = new TreeLib.AVLTreeMultiRankMap<int, TreeLib.AVLTreeMultiRankList<int>>();
            //foreach ((var vertex, (var merges, var edges)) in graph)
            //{
            //    var edgeTree = new TreeLib.AVLTreeMultiRankList<int>();
            //    int edgeTotal = 0;
            //    foreach (var edge in edges)
            //    {
            //        edgeTree.Add(edge.Key, edge.Value);
            //        edgeTotal += edge.Value;
            //    }
            //    tree.Add(vertex, edgeTree, edgeTotal);
            //}

            while (tree.LongCount > k)
            {
                //this is O(V+(E/V)) to select an edge as we have to go through all nodes
                //and then the edges for a node each time
                //can we maintain the weights better?
                //Ds = cumulative probability of selecting that node (sum of all edge weights for each node)
                //Es = cumulative probability of selecting an edge (sum of edge weights within node)
                //we remove edges connecting the two nodes, all other nodes retain their edge weights

                (var v1, var v2) = randomSelectKragersOST(tree);

                //remove all existence of the edge in the tree
                tree.Get(v1, out var v1Tree, out var v1NodeRank, out var v1NodeRankCount);
                v1Tree.Remove(v2);

                tree.Get(v2, out var v2Tree, out var v2NodeRank, out var v2NodeRankCount);
                tree.Remove(v2);

                //now we need to maintain our weight tree
                foreach (var v2Edge in v2Tree)
                {
                    var v3 = v2Edge.Key;
                    var v3Weight = v2Edge.Count;
                    if (v3 == v1)
                        //removed already
                        continue;

                    tree.Get(v3, out var v3Tree, out var v3NodeRank, out var v3NodeRankCount);
                    v3Tree.Remove(v2);

                    if (v3Tree.ContainsKey(v1))
                    {
                        v3Tree.Get(v1, out var v3EdgeTree, out int v3EdgeRank, out int v3EdgeRankCount);
                        v3Tree.Set(v1, v3Weight + v3EdgeRankCount);
                        v1Tree.Set(v3, v3Weight + v3EdgeRankCount);
                    }
                    else
                    {
                        v3Tree.Add(v1, v3Weight);
                        v1Tree.Add(v3, v3Weight);
                    }
                }
                //update v1 tree to reflect the new total edge weight
                tree.Set(v1, v1Tree, v1Tree.RankCount);

                //_2023_25.Graph.ContractEdge(graph, v1, v2);
            }
        }
        static (int vertex, int edge) randomSelectKragersOST(TreeLib.AVLTreeMultiRankMap<int, TreeLib.AVLTreeMultiRankList<int>> tree)
        {
            var edgeRank = rand.Next(tree.RankCount);
            tree.NearestLessByRank(edgeRank, out var nodeIndex);
            var edgeTreeKey = tree.GetKeyByRank(nodeIndex);
            tree.Get(edgeTreeKey, out var edgeTree, out var rank, out var rankCount);
            edgeTree.NearestLessByRank((edgeRank - rank) % rankCount, out var edgeIndex);
            var edge = edgeTree.GetKeyByRank(edgeIndex);
            return (edgeTreeKey, edge);
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

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
    public static class KargerStein_OST
    {
        static Random rand = new Random();
        static int recurseCalled = 0;
        static int contractCalled = 0;
        static int edgeTransferCount = 0;

        public static (int minCut, List<int> partition) MinimumCut(ReadOnlyDictionary<int, List<int>> graph, int FINDCUT, bool useRecursive, double reductionFactor, int stopAt)
        {
            int bestCut = int.MaxValue;
            List<int> bestPartition = null;
            while (bestCut > FINDCUT)
            {
                recurseCalled = 0;
                contractCalled = 0;
                edgeTransferCount = 0;
                var treeCache = new Dictionary<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)>();
                var tree = new AVLTreeMultiRankMap<int, (List<int> merges, RedBlackTreeMultiRankList<int>)>();
                foreach ((var vertex, var edges) in graph)
                {
                    var edgeTree = new RedBlackTreeMultiRankList<int>();
                    int edgeTotal = 0;
                    foreach (var edge in edges)
                    {
                        edgeTree.Add(edge, 1);
                        edgeTotal += 1;
                    }
                    tree.Add(vertex, ([vertex], edgeTree), edgeTotal);
                    treeCache.Add(vertex, ([vertex], edgeTree));
                }

                if (useRecursive)
                    tree = recursiveContractKargers(tree, treeCache, reductionFactor, stopAt);
                else
                    contractKargers(tree, treeCache, 2);

                var minCut = tree.First().Count;
                if (minCut < bestCut)
                {
                    bestCut = minCut;
                    bestPartition = tree.First().Value.merges;
                }

                //this is typically ~300,000 for our input of ~1500 vertexes and 6000 edges
                //~30,000 if we run the non-recursive version
                Console.WriteLine($"OST - Found cut of {bestCut} with {recurseCalled} recursions, {contractCalled} contractions, {edgeTransferCount} edges transferred to merged nodes");
            }
            return (bestCut, bestPartition);
        }

        static TreeLib.AVLTreeMultiRankMap<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)> recursiveContractKargers(TreeLib.AVLTreeMultiRankMap<int, (List<int> merges, TreeLib.RedBlackTreeMultiRankList<int> tree)> tree, Dictionary<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)> treeCache, double reductionFactor, int stopAt)
        {
            recurseCalled++;
            long N = tree.LongCount;
            if (N <= stopAt)
            {
                contractKargers(tree, treeCache, 2);
                return tree;
            }
            else
            {
                //seems sensitive to this, and in particular is quite slow
                //with the recommended factor of 1/sqrt(2), 2.1 seems to work better
                var limit = (int)(N / reductionFactor);

                //we can modify the original graph for one branch
                //but we have to take a copy for the other branch
                var g2 = new AVLTreeMultiRankMap<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)>(tree);
                var treeCache2 = new Dictionary<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)>();
                foreach (var g2Tree in g2.ToArray())
                {
                    //make sure we copy merges, as well as the tree
                    var newTree = (g2Tree.Value.merges.ToList(), new RedBlackTreeMultiRankList<int>(g2Tree.Value.edges));
                    g2.Set(g2Tree.Key, newTree, g2Tree.Count);
                    treeCache2[g2Tree.Key] = newTree;
                }

                contractKargers(tree, treeCache, limit);
                var r1 = recursiveContractKargers(tree, treeCache, reductionFactor, stopAt);

                contractKargers(g2, treeCache2, limit);
                var r2 = recursiveContractKargers(g2, treeCache2, reductionFactor, stopAt);

                return r1.First().Count < r2.First().Count ? r1 : r2;
            }
        }
        static void contractKargers(AVLTreeMultiRankMap<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)> tree, Dictionary<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)> treeCache, int k)
        {
            contractCalled++;
            while (tree.LongCount > k)
            {
                //O(ln n)
                (var v1, var v2) = randomSelectKragersOST(tree);

                //remove all existence of the edge in the tree
                var v1Tree = treeCache[v1];
                v1Tree.edges.Remove(v2);

                var v2Tree = treeCache[v2];
                tree.Remove(v2);

                //now we need to maintain our weight tree
                //for all E edges in the node to be merged (v2 -> v3 edges)
                //we perform some O(ln n) operations
                //edge moves required scales with n^2, we we are no better off than the simple algorithm
                //we we must perform n^2 ln n operations
                foreach (var v2Edge in v2Tree.edges)
                {
                    edgeTransferCount++;
                    (var v3, var v3Weight) = (v2Edge.Key, v2Edge.Count);

                    if (v3 == v1)
                        continue;

                    //O(1) lookup for our node trees as we cache them
                    //and they never change
                    var v3Tree = treeCache[v3];                    
                    
                    //Union-Find structure can be used for unweighted graphs
                    //O(ln n) to remove the edge                    
                    v3Tree.edges.Remove(v2);

                    //could have an AddOrAdjust method?
                    if (v3Tree.edges.ContainsKey(v1))
                    {
                        //and then we get the edge tree O(ln E/N)
                        v3Tree.edges.AdjustCount(v1, v3Weight);
                        v1Tree.edges.AdjustCount(v3, v3Weight);
                    }
                    else
                    {
                        v3Tree.edges.Add(v1, v3Weight);
                        v1Tree.edges.Add(v3, v3Weight);
                    }
                }
                //update v1 tree to reflect the new total edge weight
                v1Tree.merges.AddRange(v2Tree.merges);
                tree.Set(v1, v1Tree, v1Tree.edges.RankCount);
            }
        }
        static (int vertex, int edge) randomSelectKragersOST(AVLTreeMultiRankMap<int, (List<int> merges, RedBlackTreeMultiRankList<int> edges)> tree)
        {
            //O(ln V)
            var edgeRank = rand.Next(tree.RankCount);
            //could change this to return the actual value, rather than the index
            tree.NearestLessByRank(edgeRank, out var nodeIndex);
            //O(ln E/N)
            var edgeTreeKey = tree.GetKeyByRank(nodeIndex);
            tree.Get(edgeTreeKey, out var edgeTree, out var rank, out var rankCount);
            //and again, return the actual value, rather than the index
            edgeTree.edges.NearestLessByRank((edgeRank - rank) % rankCount, out var edgeIndex);
            var edge = edgeTree.edges.GetKeyByRank(edgeIndex);
            return (edgeTreeKey, edge);
        }
    }
}

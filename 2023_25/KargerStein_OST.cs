using QuikGraph;
using QuikGraph.Collections;
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
                Dictionary<int, Dictionary<int, int>> nodeCache = new Dictionary<int, Dictionary<int, int>>();
                ForestDisjointSet<int> merges = new ForestDisjointSet<int>();
                var tree = new AVLTreeMultiRankMap<int, Dictionary<int, int>>();

                foreach ((var vertex, var edges) in graph)
                {
                    var edgeTree = new Dictionary<int, int>();
                    int edgeTotal = 0;
                    foreach (var edge in edges)
                    {
                        if (edgeTree.ContainsKey(edge))
                            edgeTree[edge]++;
                        else
                            edgeTree.Add(edge, 1);
                        edgeTotal += 1;
                    }
                    tree.Add(vertex, edgeTree, edgeTotal);
                    nodeCache.Add(vertex, edgeTree);
                    merges.MakeSet(vertex);
                }

                if (useRecursive)
                    (var returnTree, var returnSet) = recursiveContractKargers(tree, nodeCache, merges, reductionFactor, stopAt);
                else
                    contractKargers(tree, nodeCache, merges, 2);

                var minCut = tree.First().Count;
                if (minCut < bestCut)
                {
                    bestCut = minCut;
                    bestPartition = nodeCache.Keys.Where(key => merges.AreInSameSet(key,tree.First().Key)).ToList();
                }

                //this is typically ~300,000 for our input of ~1500 vertexes and 6000 edges
                //~30,000 if we run the non-recursive version
                //Console.WriteLine($"OST - Found cut of {bestCut} with {recurseCalled} recursions, {contractCalled} contractions, {edgeTransferCount} edges transferred to merged nodes");
            }
            return (bestCut, bestPartition);
        }

        static (TreeLib.AVLTreeMultiRankMap<int, Dictionary<int,int>>, ForestDisjointSet<int> merges) recursiveContractKargers(TreeLib.AVLTreeMultiRankMap<int, Dictionary<int,int>> tree, Dictionary<int, Dictionary<int, int>> nodeCache, ForestDisjointSet<int> merges, double reductionFactor, int stopAt)
        {
            recurseCalled++;
            long N = tree.LongCount;
            if (N <= stopAt)
            {
                contractKargers(tree, nodeCache, merges, 2);
                return (tree, merges);
            }
            else
            {
                //seems sensitive to this, and in particular is quite slow
                //with the recommended factor of 1/sqrt(2), 2.1 seems to work better
                var limit = (int)(N / reductionFactor);

                //we can modify the original graph for one branch
                //but we have to take a copy for the other branch
                //for our Union-Find set
                //we create two new copies, and initialise them with the nodes in the tree so far
                //then when we select one to return, we merge it with the original

                var originalKeys = new List<int>();
                var set1 = new ForestDisjointSet<int>();
                var set2 = new ForestDisjointSet<int>();
                var g2 = new AVLTreeMultiRankMap<int, Dictionary<int,int>>(tree);
                var nodeCache2 = new Dictionary<int, Dictionary<int, int>>();
                foreach (var g2Tree in g2.ToArray())
                {
                    //make sure we copy merges, as well as the tree
                    var newTree = g2Tree.Value.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    g2.Set(g2Tree.Key, newTree, g2Tree.Count);
                    nodeCache2[g2Tree.Key] = newTree;
                    set1.MakeSet(g2Tree.Key);
                    set2.MakeSet(g2Tree.Key);
                    originalKeys.Add(g2Tree.Key);
                }

                contractKargers(tree, nodeCache, set1, limit);
                var r1 = recursiveContractKargers(tree, nodeCache, set1, reductionFactor, stopAt);

                contractKargers(g2, nodeCache2, set2, limit);
                var r2 = recursiveContractKargers(g2, nodeCache2, set2, reductionFactor, stopAt);

                (var returnTree, var returnSet) = r1.Item1.First().Count < r2.Item1.First().Count ? r1 : r2;

                foreach (var key in originalKeys)
                {
                    merges.Union(key, returnSet.FindSet(key));
                }

                return (returnTree, merges);
            }
        }
        static ForestDisjointSet<int> contractKargers(AVLTreeMultiRankMap<int, Dictionary<int, int>> tree, Dictionary<int, Dictionary<int, int>> nodeCache, ForestDisjointSet<int> merges, int k)
        {
            contractCalled++;
            while (tree.LongCount > k)
            {
                //O(E/N + ln N)
                (var v1, var v2) = randomSelectKragersOST(tree);

                var v1Edges = nodeCache[v1];
                //this doesn't adjust the root tree weight                
                v1Edges.Remove(v2);

                var v2Edges = nodeCache[v2];
                //this removes from the total tree weight the original sum of all v2s edges
                tree.Remove(v2);

                //now we need to maintain our weight tree
                int movedWeights = 0;
                foreach ((var v3, var v3Weight) in v2Edges)
                {
                    edgeTransferCount++;

                    if (v3 == v1)
                        continue;

                    //O(1) lookup for our node trees as we cache them
                    //and they never change
                    var v3Edges = nodeCache[v3];
                    
                    //Union-Find structure can be used for unweighted graphs
                    //O(ln n) to remove the edge                    
                    v3Edges.Remove(v2);

                    //could have an AddOrAdjust method?
                    if (v3Edges.ContainsKey(v1))
                    {
                        //and then we get the edge tree O(ln E/N)
                        v3Edges[v1] += v3Weight;
                        v1Edges[v3] += v3Weight;
                    }
                    else
                    {
                        v3Edges.Add(v1, v3Weight);
                        v1Edges.Add(v3, v3Weight);
                    }
                    //these are new v1 edges to add the weight back on
                    movedWeights += v3Weight;
                }

                //update v1 tree to reflect the new total edge weight
                merges.Union(v1, v2);
                tree.AdjustCount(v1, movedWeights - v2Edges[v1]);
            }
            return merges;
        }
        static (int vertex, int edge) randomSelectKragersOST(AVLTreeMultiRankMap<int, Dictionary<int, int>> tree)
        {
            //O(ln V)
            var edgeRank = rand.Next(tree.RankCount);
            //could change this to return the actual value, rather than the index
            tree.NearestLessByRank(edgeRank, out var nodeIndex);
            
            //O(E)
            var edgeTreeKey = tree.GetKeyByRank(nodeIndex);
            tree.Get(edgeTreeKey, out var edgeTree, out var rank, out var rankCount);

            //we have chosen a node, and index is the weighted index of the edge within that node
            //so go through them and find which the corresponding edge is
            var index = edgeRank - rank;
            int acc = 0;
            var edges = edgeTree.ToArray();
            for (int i = 0; true; i++)
            {
                acc += edges[i].Value;
                if (acc >= index)
                {
                    return (edgeTreeKey, edges[i].Key);
                }
            }
        }
    }
}

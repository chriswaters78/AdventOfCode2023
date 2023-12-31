using QuikGraph;
using System.Collections.ObjectModel;

namespace _2023_25
{
    public static class StoerWagner
    {
        public static (int minCut, List<int> partition) MinimumCut(ReadOnlyDictionary<int, List<int>> graph)
        {
            var a = graph.Keys.First();
            (int minCut, List<int> minPartition) globalResult = (int.MaxValue, null);

            var g = graph.ToDictionary(kvp => kvp.Key, kvp => ((List<int>)[kvp.Key], kvp.Value.ToDictionary(str => str, _ => 1)));
            while (g.Count > 1)
            {
                //for a given starting node, it finds the most leasely connected vertex
                //and merges that with the starting node
                //keep doing this until we have merged the entire graph
                //the least tightly connected vertex we add is our minimum cut
                var phaseResult = minimumCutPhaseStoerWagner(g, a);
                if (phaseResult.minCut < globalResult.minCut)
                    globalResult = phaseResult;
            }

            return globalResult;
        }

        static (int minCut, List<int> partition) minimumCutPhaseStoerWagner(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, int a)
        {
            var A = new List<int>(graph.Count) { a };
            //need a heap that supports updating priorities
            var weights = new QuikGraph.Collections.FibonacciHeap<int, int>(QuikGraph.Collections.HeapDirection.Decreasing);
            var cells = new Dictionary<int, QuikGraph.Collections.FibonacciHeapCell<int, int>>();
            foreach (var v in graph.Keys)
            {
                if (v == a)
                    continue;

                cells[v] = graph[v].edges.ContainsKey(a) ? weights.Enqueue(graph[v].edges[a], v) : cells[v] = weights.Enqueue(0, v);
            }
            int cutOfPhase = int.MaxValue;

            while (A.Count != graph.Count)
            {
                //find the most strongly connected vertex to A, lg n to remove the min from the fibonnacci heap
                (cutOfPhase, var next) = weights.Dequeue();
                cells.Remove(next);
                A.Add(next);

                //Update the fibbonacci heap of weights
                //Every vertex connected to next that is not already merged
                //Must have its priority increased by the edge weight
                //as these are now directly connected to A and is a candidate for selection
                foreach ((var connected, var count) in graph[next].edges)
                {
                    if (cells.ContainsKey(connected))
                    {
                        //O(1) to increase key on a fibbonacci heap based queue
                        weights.ChangeKey(cells[connected], cells[connected].Priority + count);
                    }
                }
            }

            //the last node we added represents the mininum cut for this phase
            //so return the list of nodes that were merged into it
            //before we contract it with our existing seed vertex
            var partition = graph[A[A.Count - 1]].merges;
            
            //Merging the last two added nodes, this is the least tightly connected vertex
            //and so maintains the property that all we know the min cut of all nodes edge-contracted with a
            _2023_25.Graph.ContractEdge(graph, A[A.Count - 2], A[A.Count - 1]);
            return (cutOfPhase, partition);
        }

        //this is the example weighted graph from their paper
        //https://citeseerx.ist.psu.edu/document?repid=rep1&type=pdf&doi=b10145f7fc3d07e43607abc2a148e58d24ced543
        public static Dictionary<string, Dictionary<string, int>> TestGraph = new ()
        {
            { "one", new Dictionary<string, int>() { { "two", 2 }, { "five", 3 } } },
            { "two", new Dictionary<string, int>() { { "one", 2 }, { "five", 2 }, { "six", 2 }, { "three", 3 } } },
            { "three", new Dictionary<string, int>() { { "two", 3 }, { "four", 4 }, { "seven", 2 } }},
            { "four", new Dictionary<string, int>() { { "three", 4 }, { "seven", 2 }, { "eight", 2 } }},
            { "five", new Dictionary<string, int>() { { "one", 3 }, { "two", 2 }, { "six", 3 } }},
            { "six", new Dictionary<string, int>() { { "five", 3 }, { "two", 2 }, { "seven", 1 } }},
            { "seven", new Dictionary<string, int>() { { "six", 1 }, { "three", 2 }, { "four", 2 }, { "eight", 3 } }},
            { "eight", new Dictionary<string, int>() { { "seven", 3 }, { "four", 2 } }},
        };
    }
}

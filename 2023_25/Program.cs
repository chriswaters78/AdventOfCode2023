using System.Collections.Generic;
using System.Diagnostics;
double Sqrt2 = Math.Sqrt(2);
Random rand = new Random();
var stopwatch = new Stopwatch();

var originalGraphStr = new Dictionary<string, List<string>>();
foreach (var line in File.ReadAllLines("input.txt"))
{
    //jqt: rhn xhk nvd
    var sp = line.Split(": ");
    if (!originalGraphStr.ContainsKey(sp[0]))
        originalGraphStr[sp[0]] = new List<string>();
    foreach (var edge in sp[1].Split(" "))
    {
        originalGraphStr[sp[0]].Add(edge);
        if (!originalGraphStr.ContainsKey(edge))
            originalGraphStr[edge] = new List<string>();
        originalGraphStr[edge].Add(sp[0]);
    }
}

var intToKeyMap = originalGraphStr.Keys.Select((str, i) => (str, i)).ToDictionary(tp => tp.i, tp => tp.str).AsReadOnly();
var keyToIntMap = originalGraphStr.Keys.Select((str, i) => (str, i)).ToDictionary(tp => tp.str, tp => tp.i).AsReadOnly();
var originalGraph = originalGraphStr.ToDictionary(kvp => keyToIntMap[kvp.Key], kvp => kvp.Value.Select(edge => keyToIntMap[edge]).ToList());

Console.WriteLine($"{originalGraph.Keys.Count} nodes, edges {originalGraph.Values.Sum(list => list.Count)}");

var tests = new (string name, Func<(int minCut, List<int> partition)>)[] {
        //("Count crossings", () => countCrossings(originalGraph)),
        //("Handrolled", () => method1(originalGraph)),
        //("Stoer-Wagner", () => minimumCutStoerWagner(originalGraph)),
        //("Karger-Stein 1", () => kargers(originalGraph, 3, true, 1.4, 7)),
        ("Karger-Stein 1.8", () => kargers(originalGraph, 3, true, 1.8, 6)),
        ("Karger-Stein 1.9", () => kargers(originalGraph, 3, true, 1.9, 6)),
        ("Karger-Stein 2.0", () => kargers(originalGraph, 3, true, 2.0, 6)),
        ("Karger-Stein 2.1", () => kargers(originalGraph, 3, true, 2.1, 6)),
        ("Karger-Stein 2.2", () => kargers(originalGraph, 3, true, 2.2, 6)),
        ("Karger-Stein 2.3", () => kargers(originalGraph, 3, true, 2.3, 6)),
        //("Karger-Stein 6", () => kargers(originalGraph, 3, true, 3, 6)),
        //("Karger", () => kargers(originalGraph, 3, false))
    };

var averageRuntimes = new long[tests.Length];
HashSet<string> prevSet1 = null;
HashSet<string> prevSet2 = null;
for (int i = 0; i < 100; i++)
{
    Console.WriteLine($"########### Iteration {i} ###########");
    foreach (var ((name, resultFactory), index) in tests.Select((test,i) => (test,i)))
    {
        stopwatch.Restart();
        (var minCut, List<int> partition) = resultFactory();
        stopwatch.Stop();
        averageRuntimes[index] = (averageRuntimes[index] * i + stopwatch.ElapsedMilliseconds) / (i + 1);
        var set1 = partition.Select(i => intToKeyMap[i]).ToHashSet();
        var set2 = originalGraphStr.Keys.Where(key => !set1.Contains(key)).ToHashSet();
        
        Console.WriteLine($"{name}: min cut {minCut}, partition {set1.Count} <=> {set2.Count}, average: {averageRuntimes[index]}ms, this: {stopwatch.ElapsedMilliseconds}ms");

        (set1, set2) = set1.Count <= set2.Count ? (set1, set2) : (set2, set1);
        if (prevSet1 != null)
        {
            if (!prevSet1.OrderBy(str => str).SequenceEqual(set1.OrderBy(str => str)))
            {
                Console.WriteLine("*** set did not match previous found set ***");
            }
        }
        else
        {
            Console.WriteLine($"Found two sets with mincut of {minCut}");
            Console.WriteLine($"Set 1: [{String.Join(",", set1)}]");
            Console.WriteLine($"Set 2: [{String.Join(",", set2)}]");
            
            var edges = originalGraphStr.SelectMany(kvp => kvp.Value.Select(to => (kvp.Key, to)))
                .Where(tp => set1.Contains(tp.Key) && set2.Contains(tp.to) || set2.Contains(tp.Key) && set1.Contains(tp.to))
                .Select(tp => tp.Key.CompareTo(tp.to) == -1 ? (tp.Key, tp.to) : (tp.to, tp.Key))
                .Distinct().ToList<(string from, string to)>();
            
            Console.WriteLine($"Min cut edges = {String.Join("; ", edges.Select(tp => $"{tp.from} => {tp.to}"))}");
        }
        (prevSet1, prevSet2) = (set1, set2);
    }
}
(int minCut, List<int> partition) kargers(Dictionary<int, List<int>> graph, int FINDCUT, bool useRecursive, double reductionFactor, int stopAt)
{
    int bestCut = int.MaxValue;
    List<int> bestPartition = null;
    stopwatch.Restart();
    while (bestCut != FINDCUT)
    {
        var result = useRecursive ? recursiveContractKargers(graph.ToDictionary(kvp => kvp.Key, kvp => ((List<int>)[kvp.Key], kvp.Value.ToDictionary(str => str, _ => 1))), reductionFactor, stopAt)
            : contractKargers(graph.ToDictionary(kvp => kvp.Key, kvp => ((List<int>)[kvp.Key], kvp.Value.ToDictionary(str => str, _ => 1))), 2);

        var minCut = result.Values.First().edges.Sum(kvp => kvp.Value);
        if (minCut < bestCut)
        {
            bestCut = minCut;
            bestPartition = result.First().Value.merges;
        }
    }
    return (bestCut, bestPartition);
}

Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> recursiveContractKargers(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, double reductionFactor, int stopAt)
{
    int N = graph.Count;
    if (N <= stopAt)
    {
        var g = graph.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.merges.ToList(), kvp.Value.edges.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)));
        g = contractKargers(g, 2);
        return g;
    }
    else
    {
        var limit = (int) (N / reductionFactor);
        if (limit >= N)
        {
            throw new($"N: {N}, reduction {reductionFactor} => limit {limit}, not reduced");
        }

        var g1 = graph.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.merges.ToList(), kvp.Value.edges.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)));
        g1 = contractKargers(g1, limit);
        var r1 = recursiveContractKargers(g1, reductionFactor, stopAt);

        var g2 = graph.ToDictionary(kvp => kvp.Key, kvp => (kvp.Value.merges.ToList(), kvp.Value.edges.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value)));
        g2 = contractKargers(g2, limit);
        var r2 = recursiveContractKargers(g2, reductionFactor, stopAt);

        return r1.Values.First().edges.Sum(kvp => kvp.Value) < r2.Values.First().edges.Sum(kvp => kvp.Value) ? r1 : r2;
    }
}
Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> contractKargers(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, int k)
{
    while (graph.Count > k)
    {
        (var vertex, var edge) = randomSelectKragers(graph);
        graph = contractEdge(graph, vertex, edge);
    }

    return graph;
}

(int minCut, List<int>) countCrossings(Dictionary<int,List<int>> graph)
{
    var crossingCounts = new Dictionary<(int from, int to), int>();
    for (int i = 0; i < 5000; i++)
    {
        var v1 = graph.Keys.ToArray()[rand.Next(graph.Count)];
        var v2 = graph.Keys.Where(key => key != v1).ToArray()[rand.Next(graph.Count - 1)];

        var path = shortestPath(graph, v1, v2);
        for (int p = 1; p < path.Count; p++)
        {
            var key = path[p-1].CompareTo(path[p]) == -1 ? (path[p - 1], path[p]) : (path[p], path[p-1]);
            if (!crossingCounts.ContainsKey(key))
                crossingCounts[key] = 0;

            crossingCounts[key]++;
        }
    }

    var topThree = crossingCounts.OrderByDescending(kvp => kvp.Value).Take(3).ToList();

    //remove these 3 edges
    graph = graph.ToDictionary(KeyValuePair => KeyValuePair.Key, KeyValuePair => KeyValuePair.Value.ToList());
    graph[topThree[0].Key.from].Remove(topThree[0].Key.to);
    graph[topThree[0].Key.to].Remove(topThree[0].Key.from);
    graph[topThree[1].Key.from].Remove(topThree[1].Key.to);
    graph[topThree[1].Key.to].Remove(topThree[1].Key.from);
    graph[topThree[2].Key.from].Remove(topThree[2].Key.to);
    graph[topThree[2].Key.to].Remove(topThree[2].Key.from);

    var reachable = new HashSet<int>();
    var seed = graph.Keys.First();
    reachable.Add(seed);
    foreach (var v in graph.Keys.Skip(1))
    {
        if (shortestPath(graph, seed, v) != null)
        {
            reachable.Add(v);
        }
    }
    return (3, reachable.ToList());
}

//this is the examle graph from their paper
//https://citeseerx.ist.psu.edu/document?repid=rep1&type=pdf&doi=b10145f7fc3d07e43607abc2a148e58d24ced543
//var testGraph = new Dictionary<string, Dictionary<string, int>>();
//testGraph["one"] = new Dictionary<string, int>() { { "two", 2 }, { "five", 3 } };
//testGraph["two"] = new Dictionary<string, int>() { { "one", 2 }, { "five", 2 }, { "six", 2 }, { "three", 3 } };
//testGraph["three"] = new Dictionary<string, int>() { { "two", 3 }, { "four", 4 }, { "seven", 2 } };
//testGraph["four"] = new Dictionary<string, int>() { { "three", 4 }, { "seven", 2 }, { "eight", 2 } };
//testGraph["five"] = new Dictionary<string, int>() { { "one", 3 }, { "two", 2 }, { "six", 3 } };
//testGraph["six"] = new Dictionary<string, int>() { { "five", 3 }, { "two", 2 }, { "seven", 1 } };
//testGraph["seven"] = new Dictionary<string, int>() { { "six", 1 }, { "three", 2 }, { "four", 2 }, { "eight", 3 } };
//testGraph["eight"] = new Dictionary<string, int>() { { "seven", 3 }, { "four", 2 } };
//minimumCutStoerWagner(testGraph, "two");

(int minCut, List<int> partition) minimumCutStoerWagner(Dictionary<int, List<int>> graph)
{
    var a = graph.Keys.ToArray()[rand.Next(graph.Count)];
    var minCut = int.MaxValue;
    List<int> minPartition = null;

    var g = graph.ToDictionary(kvp => kvp.Key, kvp => ((List<int>) [kvp.Key], kvp.Value.ToDictionary(str => str, _ => 1)));
    while (g.Count > 1)
    {
        (var cutOfPhase, var partition) = minimumCutPhaseStoerWagner(g, a);
        if (cutOfPhase < minCut)
        {
            minCut = cutOfPhase;
            minPartition = partition;
        }
    }

    return (minCut, minPartition);
}
(int minCut, List<int> partition) minimumCutPhaseStoerWagner(Dictionary<int, (List<int> merges, Dictionary<int,int> edges)> graph, int a)
{
    var A = new List<int>();
    A.Add(a);
    //need a heap that supports updating priorities
    var weights = new QuikGraph.Collections.FibonacciHeap<int, int>(QuikGraph.Collections.HeapDirection.Decreasing);
    var cells = new Dictionary<int, QuikGraph.Collections.FibonacciHeapCell<int, int>>();
    foreach (var v in graph.Keys)
    {
        if (v == a)
            continue;

        if (graph[v].edges.ContainsKey(a))
        {
            cells[v] = weights.Enqueue(graph[v].edges[a], v);
        }
        else
        {
            cells[v] = weights.Enqueue(0, v);
        }
    }
    int cutOfPhase = int.MaxValue;

    //n iterations
    while (A.Count != graph.Count)
    {
        //find the most strongly connected vertex to A
        //lg n to remove the min
        (cutOfPhase, var next) = weights.Dequeue();
        cells.Remove(next);
        A.Add(next);
        //update queue, every vertex connected to next must have its priority increased by the weight of edge to next
        //O(E)
        foreach ((var connected, var count) in graph[next].edges)
        {
            if (cells.ContainsKey(connected))
            {
                //O(1) to increase key?
                weights.ChangeKey(cells[connected], cells[connected].Priority + count);
            }
        }
    }

    var partition = graph[A[A.Count - 1]].merges;
    //now we must shrink G by merging the last two added nodes
    graph = contractEdge(graph, A[A.Count - 2], A[A.Count - 1]);
    return (cutOfPhase, partition);
}


(int vertex, int edge) randomSelectKragers(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph)
{
    //find weights for all keys
    var Ds = new List<(int key, int cumulativeWeight)>();
    int acc = 0;
    foreach (var kvp in graph)
    {
        acc += kvp.Value.edges.Count;
        Ds.Add((kvp.Key, acc));
    }
    var randomVertex = rand.Next(acc);
    int chosenVertex = int.MinValue;
    foreach (var d in Ds)
    {
        if (randomVertex < d.cumulativeWeight)
        {
            chosenVertex = d.key;
            break;
        }
    }
    var Es = new List<(int key, int cumulativeWeight)>();
    acc = 0;
    foreach (var kvp in graph[chosenVertex].edges)
    {
        acc += kvp.Value;
        Es.Add((kvp.Key, acc));
    }
    var randomEdge = rand.Next(acc);
    int chosenEdge = int.MinValue;
    foreach (var e in Es)
    {
        if (randomEdge < e.cumulativeWeight)
        {
            chosenEdge = e.key;
            break;
        }
    }

    return (chosenVertex, chosenEdge);
}

(int minCut, List<int> partition) method1(Dictionary<int,List<int>> graph)
{
    stopwatch.Restart();
    graph = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 
    var result = contract(graph);

    var minCut = result.First().Value.edges.Sum(tp => tp.Value);
    var partition = result.Values.First().merges;

    return (minCut, partition);
}

Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> contractEdge(Dictionary<int, (List<int> merges, Dictionary<int, int> edges)> graph, int v1, int v2)
{
    //we merge everything into v1 and keep that key
    //and recorded any merges in the merges list for that node
    //this is more efficient that using long merged strings in all the edge dictionaries
    var newV1 = graph[v1].edges.Concat(graph[v2].edges).Where(e => e.Key != v1 && e.Key != v2)
        .GroupBy(kvp => kvp.Key).ToDictionary(grp => grp.Key, grp => grp.Sum(kvp => kvp.Value));

    //find all the edges that previously joined to v1
    //these need to join to newKey instead
    foreach (var edge in graph[v1].edges)
    {
        if (edge.Key == v2)
            continue;

        graph[edge.Key].Item2[v1] = edge.Value;
    }
    foreach (var edge in graph[v2].edges)
    {
        if (edge.Key == v1)
            continue;

        graph[edge.Key].edges[v1] = graph[edge.Key].edges.ContainsKey(v1) ? graph[edge.Key].edges[v1] + edge.Value : edge.Value;
        graph[edge.Key].edges.Remove(v2);
    }

    graph[v1] = (graph[v1].merges, newV1);
    graph[v1].merges.AddRange(graph[v2].merges);
    graph.Remove(v2);

    return graph;
}

//Dictionary<string, Dictionary<string, int>> contractEdge(Dictionary<string, Dictionary<string, int>> graph, string v1, string v2)
//{
//    //the keys grow quite long in this version
//    //better to convert to int keys, and then keep a seperate mapping of ints to strings that can be updated
//    //when we merge things
//    var newKey = $"{v1}{v2}";
//    graph[newKey] = graph[v1].Concat(graph[v2]).Where(e => e.Key != v1 && e.Key != v2)
//        .GroupBy(kvp => kvp.Key).ToDictionary(grp => grp.Key, grp => grp.Sum(kvp => kvp.Value));

//    //find all the edges that previously joined to v1
//    //these need to join to newKey instead
//    foreach (var edge in graph[v1])
//    {
//        if (edge.Key == v2)
//            continue;

//        graph[edge.Key][newKey] = edge.Value;
//        graph[edge.Key].Remove(v1);
//    }
//    foreach (var edge in graph[v2])
//    {
//        if (edge.Key == v1)
//            continue;

//        graph[edge.Key][newKey] = graph[edge.Key].ContainsKey(newKey) ? graph[edge.Key][newKey] + edge.Value : edge.Value;
//        graph[edge.Key].Remove(v2);
//    }
//    graph.Remove(v1);
//    graph.Remove(v2);

//    return graph;
//}


Dictionary<int, (List<int> merges, Dictionary<int,int> edges)> contract(Dictionary<int, List<int>> graph)
{
    var g = graph.ToDictionary(kvp => kvp.Key, kvp => (new List<int>() { kvp.Key }, kvp.Value.ToDictionary(key => key, _ => 1)));
    var rand = new Random();
    while (g.Count > 2)
    {
        //we have to choose nodes randomly because we only change the graph
        //if we find 4 independent routes
        int key1 = g.Keys.ToList()[rand.Next(g.Count)];
        int key2 = g[key1].Item2.ToList()[rand.Next(g[key1].Item2.Count - 1)].Key;

        var without = new Dictionary<(int from, int to), int>();
        List<(int from, int to)> path = null;
        for (int count = 0; count < 4; count++)
        {
            path = canReachWithoutEdges(g, key1, key2, without);
            if (path == null)
                break;

            foreach (var edge in path)
            {
                if (!without.ContainsKey(edge))
                    without[edge] = 0;

                without[edge]++;
            }
        }
        if (path != null)
        {
            g = contractEdge(g, key1, key2);
        }
    }
    return g;
}
List<int> shortestPath(Dictionary<int, List<int>> graph, int start, int end)
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

List<(int from, int to)> canReachWithoutEdges(Dictionary<int, (List<int> merges, Dictionary<int,int> edges)> graph, int start, int end, Dictionary<(int from, int to), int> without)
{
    var visited = new HashSet<int>();
    var queue = new Queue<(int, List<(int from, int to)>, int)>();
    visited.Add(start);
    queue.Enqueue((start, new List<(int from, int to)>(), 0));
    while (queue.Any())
    {
        (var curr, var route, var depth) = queue.Dequeue();
        if (curr == end)
        {
            return route;
        }

        foreach ((var edge, var edgeCount) in graph[curr].edges)
        {
            (var from, var to) = curr.CompareTo(edge) == -1 ? (curr, edge) : (edge, curr);
            if (without.ContainsKey((from, to)) && without[(from,to)] >= edgeCount)
                continue;

            if (visited.Contains(edge))
                continue;

            visited.Add(edge);
            var newRoute = route.ToList();
            newRoute.Add((from, to));
            queue.Enqueue((edge, newRoute, depth + 1));
        }
    }
    return null;
}
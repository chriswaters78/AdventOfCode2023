using System.Collections.Generic;
using System.Diagnostics;
double Sqrt2 = Math.Sqrt(2);
Random rand = new Random();
var stopwatch = new Stopwatch();

//this is the example graph from their paper
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


var graph = new Dictionary<string, List<string>>();
foreach (var line in File.ReadAllLines("input.txt"))
{
    //jqt: rhn xhk nvd
    var sp = line.Split(": ");
    if (!graph.ContainsKey(sp[0]))
        graph[sp[0]] = new List<string>();
    foreach (var edge in sp[1].Split(" "))
    {
        graph[sp[0]].Add(edge);
        if (!graph.ContainsKey(edge))
            graph[edge] = new List<string>();
        graph[edge].Add(sp[0]);
    }
}

Console.WriteLine($"{graph.Keys.Count} nodes, edges {graph.Values.Sum(list => list.Count)}");

HashSet<string> prevSet1 = null;
HashSet<string> prevSet2 = null;
for (int i = 0; i < 100; i++)
{
    foreach (var (name, resultFactory) in new (string name, Func<(int minCut, string partition)>)[] {
        ("Stoer-Wagner", () => minimumCutStoerWagner(graph)),
        ("Kragers (recursive)", () => kragers(graph, 3, true)),
        ("Kragers (non-recursive)", () => kragers(graph, 3, false))})
    {
        stopwatch.Restart();
        (var minCut, string partition) = resultFactory();
        var set1 = partition.Select((ch, i) => (ch, i)).GroupBy(tp => tp.i / 3).Select(grp => new String(grp.OrderBy(tp => tp.i).Select(tp => tp.ch).ToArray())).ToHashSet();
        var set2 = graph.Keys.Where(key => !set1.Contains(key)).ToHashSet();


        Console.WriteLine($"{name} found min cut of {minCut}, between a partition of size {set1.Count} and {set2.Count} in {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"{name} Part2: {set1.Count * set2.Count}");

        (set1, set2) = set1.Count <= set2.Count ? (set1, set2) : (set2, set1);
        if (prevSet1 != null)
        {
            if (!prevSet1.OrderBy(str => str).SequenceEqual(set1.OrderBy(str => str)))
            {
                Console.WriteLine("Set did not match previous found set");
            }
            else
            {
                Console.WriteLine("Sets matched previous minCut found");
            }
        }
        (prevSet1, prevSet2) = (set1, set2);
    }
}

(int minCut, string partition) minimumCutStoerWagner(Dictionary<string, List<string>> graph)
{
    var a = graph.Keys.ToArray()[rand.Next(graph.Count)];
    var minCut = int.MaxValue;
    string minPartition = null;

    var g = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(str => str, _ => 1));
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
(int minCut, string partition) minimumCutPhaseStoerWagner(Dictionary<string, Dictionary<string,int>> graph, string a)
{
    var A = new List<string>();
    A.Add(a);
    var weights = new Dictionary<string, int>();
    foreach (var v in graph.Keys)
    {
        if (v == a)
            continue;

        if (graph[v].ContainsKey(a))
        {
            weights[v] = graph[v][a];
        }
        else
        {
            weights[v] = 0;
        }
    }
    int cutOfPhase = int.MaxValue;
    while (A.Count != graph.Count)
    {
        //find the most strongly connected vertex to A
        (var next, cutOfPhase) = weights.MaxBy(kvp => kvp.Value);
        weights.Remove(next);
        A.Add(next);
        //update queue, every vertex connected to next must have its priority increased by the weight of edge to next
        foreach ((var connected, var count) in graph[next])
        {
            if (weights.ContainsKey(connected))
            {
                weights[connected] = weights[connected] + count;
            }
        }
    }

    //now we must shrink G by merging the last two added nodes
    graph = contractEdge(graph, A[A.Count - 2], A[A.Count - 1]);
    return (cutOfPhase, A.Last());
}
(int, string) kragers(Dictionary<string, List<string>> grap, int FINDCUT, bool useRecursive)
{
    int bestCut = int.MaxValue;
    string bestPartition = null;
    stopwatch.Restart();
    while (bestCut != FINDCUT)
    {
        var result = useRecursive ? recursiveContractKragers(graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(str => str, _ => 1)))
            : contractKragers(graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(str => str, _ => 1)), 2); ;
        
        var minCut = result.Values.First().Sum(kvp => kvp.Value);
        if (minCut < bestCut)
        {
            bestCut = minCut;
            bestPartition = result.Keys.First();
        }
        //Console.WriteLine($"Found min cut of 3 found between {result.Keys.First()} and {result.Keys.Skip(1).Single()}");
        //Console.WriteLine($"{result.Keys.First().Length * result.Keys.Skip(1).Single().Length / 9}");
    }
    return (bestCut, bestPartition);
}

Dictionary<string, Dictionary<string, int>> recursiveContractKragers(Dictionary<string, Dictionary<string,int>> graph)
{
    int N = graph.Count;
    if (N < 6)
    {
        var g = graph.ToDictionary(KeyValuePair => KeyValuePair.Key, KeyValuePair => KeyValuePair.Value.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value));
        g = contractKragers(g, 2);
        return g;
    }
    else
    {
        var limit = (int)Math.Ceiling(N / (Sqrt2 + 1));
        
        var g1 = graph.ToDictionary(KeyValuePair => KeyValuePair.Key, KeyValuePair => KeyValuePair.Value.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value));
        g1 = contractKragers(g1, limit);
        var r1 = recursiveContractKragers(g1);
        
        var g2 = graph.ToDictionary(KeyValuePair => KeyValuePair.Key, KeyValuePair => KeyValuePair.Value.ToDictionary(kvp2 => kvp2.Key, kvp2 => kvp2.Value));
        g2 = contractKragers(g2, limit);
        var r2 = recursiveContractKragers(g2);

        return r1.Values.First().Sum(kvp => kvp.Value) < r2.Values.First().Sum(kvp => kvp.Value) ? r1 : r2;
    }
}

Dictionary<string, Dictionary<string, int>> contractKragers(Dictionary<string, Dictionary<string, int>> graph, int k)
{
    while (graph.Count > k)
    {
        (var vertex, var edge) = randomSelectKragers(graph);
        graph = contractEdge(graph, vertex, edge);
    }

    return graph;
}

(string vertex, string edge) randomSelectKragers(Dictionary<string, Dictionary<string, int>> graph)
{
    //find weights for all keys
    var Ds = new List<(string key, int cumulativeWeight)>();
    int acc = 0;
    foreach (var kvp in graph)
    {
        acc += kvp.Value.Count;
        Ds.Add((kvp.Key, acc));
    }
    var randomVertex = rand.Next(acc);
    string chosenVertex = null;
    foreach (var d in Ds)
    {
        if (randomVertex < d.cumulativeWeight)
        {
            chosenVertex = d.key;
            break;
        }
    }
    var Es = new List<(string key, int cumulativeWeight)>();
    acc = 0;
    foreach (var kvp in graph[chosenVertex])
    {
        acc += kvp.Value;
        Es.Add((kvp.Key, acc));
    }
    var randomEdge = rand.Next(acc);
    string chosenEdge = null;
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

void method1(Dictionary<string,List<string>> graph)
{
    graph = graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 
    //doesnt seem to always finish, buggy?
    //contract(7);

    var node1 = graph.Keys.First();
    var node2 = graph.Keys.Skip(1).First();
    Console.WriteLine($"Contracted graph to 2 nodes: {node1} (count = {node1.Length / 3}) and {node2} (count = {node2.Length / 3}) ");
    Console.WriteLine($"Part2: {(node1.Length / 3) * (node2.Length / 3)} in {stopwatch.ElapsedMilliseconds}ms");
}

Dictionary<string, Dictionary<string, int>> contractEdge(Dictionary<string, Dictionary<string, int>> graph, string v1, string v2)
{
    var newKey = $"{v1}{v2}";
    graph[newKey] = graph[v1].Concat(graph[v2]).Where(e => e.Key != v1 && e.Key != v2)
        .GroupBy(kvp => kvp.Key).ToDictionary(grp => grp.Key, grp => grp.Sum(kvp => kvp.Value));

    //find all the edges that previously joined to v1
    //these need to join to newKey instead
    foreach (var edge in graph[v1])
    {
        if (edge.Key == v2)
            continue;

        graph[edge.Key][newKey] = edge.Value;
        graph[edge.Key].Remove(v1);
    }
    foreach (var edge in graph[v2])
    {
        if (edge.Key == v1)
            continue;

        graph[edge.Key][newKey] = graph[edge.Key].ContainsKey(newKey) ? graph[edge.Key][newKey] + edge.Value : edge.Value;
        graph[edge.Key].Remove(v2);
    }
    graph.Remove(v1);
    graph.Remove(v2);

    return graph;
}


//original hacked up version
//should work but seems inconsistent?
//void contract(int maxDepth)
//{
//    var rand = new Random();
//    while (graph.Count > 2)
//    {
//        string key1 = graph.Keys.ToList()[rand.Next(graph.Count)];
//        string key2 = graph[key1].Skip(1 + rand.Next(graph[key1].Count - 1)).First();

//        var without = new Dictionary<(string from, string to), int>();
//        List<(string from, string to)> path = null;
//        for (int count = 0; count < 4; count++)
//        {
//            path = canReachWithoutEdges(maxDepth, key1, key2, without);
//            if (path == null)
//                break;

//            foreach (var edge in path)
//            {
//                if (!without.ContainsKey(edge))
//                    without[edge] = 0;

//                without[edge]++;
//            }
//        }
//        if (path != null)
//        {
//            graph = contractEdge(graph, key1, key2);
//            Console.WriteLine($"Created node {key1}{key2}, nodes left {graph.Count}");
//        }
//    }
//}

List<(string from, string to)> canReachWithoutEdges(int maxDepth, string start, string end, Dictionary<(string from, string to), int> without)
{
    var queue = new Queue<(string, HashSet<string>, List<(string from, string to)>, int)>();
    queue.Enqueue((start, [start], new List<(string from, string to)>(), 0));
    while (queue.Any())
    {
        (var curr, var visited, var route, var depth) = queue.Dequeue();
        if (curr == end)
        {
            return route;
        }

        if (depth > maxDepth)
            return null;

        foreach ((var edge, var edgeCount) in graph[curr].GroupBy(to => to).Select(grp => (grp.Key, grp.Count())).ToList())
        {
            (var from, var to) = curr.CompareTo(edge) == -1 ? (curr, edge) : (edge, curr);
            if (without.ContainsKey((from, to)) && without[(from,to)] >= edgeCount)
                continue;

            if (visited.Contains(edge))
                continue;

            var newVisited = visited.ToHashSet();
            newVisited.Add(edge);
            var newRoute = route.ToList();
            newRoute.Add((from, to));
            queue.Enqueue((edge, newVisited, newRoute, depth + 1));
        }
    }
    return null;
}
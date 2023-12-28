using System.Diagnostics;
Random rand = new Random();
var stopwatch = Stopwatch.StartNew();

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

kragers(graph);

void kragers(Dictionary<string, List<string>> grap)
{
    int bestCut = int.MaxValue;
    stopwatch.Restart();
    for (int n = 1; n < 1000000; n++)
    {
        var result = KragerMininumCut(graph.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToDictionary(str => str, _ => 1)));
        var minCut = result.Values.First().Sum(kvp => kvp.Value);
        if (minCut < bestCut)
        {
            bestCut = minCut;
            Console.WriteLine($"Cut of {bestCut} found on iteration {n} in {stopwatch.ElapsedMilliseconds}");
        }
        if (minCut == 3)
        {
            Console.WriteLine($"Found min cut of 3 found between {result.Keys.First()} and {result.Keys.Skip(1).Single()}");
            Console.WriteLine($"{result.Keys.First().Length * result.Keys.Skip(1).Single().Length / 9}");
            break;
        }
        else
        {
            Console.WriteLine($"Iteration {n} found cut of {minCut}, average time per iteration {stopwatch.ElapsedMilliseconds / n}ms");
        }
    }
}


Dictionary<string, Dictionary<string, int>> KragerMininumCut(Dictionary<string, Dictionary<string, int>> graph)
{
    while (graph.Count > 2)
    {
        (var vertex, var edge) = randomSelect(graph);
        graph = contractEdge(graph, vertex, edge);
    }

    return graph;
}

(string vertex, string edge) randomSelect(Dictionary<string, Dictionary<string, int>> graph)
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
        graph[edge.Key][newKey] = graph[edge.Key][v1];
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
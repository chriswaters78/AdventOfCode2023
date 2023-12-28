using System.Diagnostics;

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

Console.WriteLine($"{graph.Keys.Count} nodes, node1: {graph.Keys.First()}, node2: {graph.Keys.Skip(1).First()}");

//6 seems to be the optimum depth to limit to
contract(6);

var node1 = graph.Keys.First();
var node2 = graph.Keys.Skip(1).First();
Console.WriteLine($"Contract graph to 2 nodes: {node1} (count = {node1.Length / 3}) and {node2} (count = {node2.Length / 3}) ");
Console.WriteLine($"Part2: {(node1.Length / 3) * (node2.Length / 3)} in {stopwatch.ElapsedMilliseconds}ms");

void contract(int maxDepth)
{
    var rand = new Random();
    while (graph.Count > 2)
    {
        string key1 = graph.Keys.ToList()[rand.Next(graph.Count)];
        string key2 = graph[key1].Skip(1 + rand.Next(graph[key1].Count - 2)).First();

        var without = new Dictionary<(string from, string to), int>();
        List<(string from, string to)> path = null;
        for (int count = 0; count < 4; count++)
        {
            path = canReachWithoutEdges(maxDepth, key1, key2, without);
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
            graph = contractEdge(graph, key1, key2);
            Console.WriteLine($"Created node {key1}{key2}, nodes left {graph.Count}");
        }
    }
}

Dictionary<string, List<string>> contractEdge(Dictionary<string, List<string>> graph, string v1, string v2)
{
    var newKey = $"{v1}{v2}";
    graph[newKey] = graph[v1].Concat(graph[v2]).Where(e => e != v1 && e != v2).ToList();

    //find all the edges that previously joined to v1
    //these need to join to newKey instead
    foreach (var edge in graph[v1].ToList())
    {
        if (edge == v2)
            continue;

        graph[edge].Remove(v1);
        graph[edge].Add(newKey);
    }
    foreach (var edge in graph[v2].ToList())
    {
        if (edge == v1)
            continue;

        graph[edge].Remove(v2);
        graph[edge].Add(newKey);
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
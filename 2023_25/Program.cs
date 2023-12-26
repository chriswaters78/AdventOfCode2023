var graphStr = new Dictionary<string, HashSet<string>>();
using (StreamWriter writer = new StreamWriter("graph.viz"))
{
    writer.WriteLine("graph {");

    foreach (var line in File.ReadAllLines("input.txt"))
    {
        //jqt: rhn xhk nvd
        var sp = line.Split(": ");
        if (!graphStr.ContainsKey(sp[0]))
            graphStr[sp[0]] = new HashSet<string>();
        foreach (var edge in sp[1].Split(" "))
        {
            writer.WriteLine($"{sp[0]} -- {edge}");
            graphStr[sp[0]].Add(edge);
            if (!graphStr.ContainsKey(edge))
                graphStr[edge] = new HashSet<string>();
            graphStr[edge].Add(sp[0]);
        }
    }

    writer.WriteLine("}");
}

var strToIndex = graphStr.Keys.Select((str, i) => (str, i)).ToDictionary(tp => tp.str, tp => tp.i);

var graph = graphStr.ToDictionary(kvp => strToIndex[kvp.Key], kvp => kvp.Value.Select(str => strToIndex[str]).ToList());


Console.WriteLine($"{graph.Keys.Count} nodes, {graph.Values.Sum(l => l.Count)} edges");

var key1 = graph.Keys.First();
var routeCounts = (from key2 in graph.Keys
                   where key2 != key1
                   select (key1, key2, countRoutes(key1, key2))).ToList();

var set1 = new HashSet<int>();
set1.Add(key1);
var set2 = new HashSet<int>();
foreach (var route in routeCounts)
{
    if (route.Item3 >= 4)
    {
        set1.Add(route.key2);
    }
    else
    {
        set2.Add(route.key2);
    }
}

Console.WriteLine($"Part1: {set1.Count * set2.Count}");

int countRoutes(int key1, int key2)
{
    var without = new HashSet<(int from, int to)>();
    int count = 0;
    while (true)
    {
        var path = canReachWithoutEdges(key1, key2, without);
        if (path == null)
        {
            break;
        }
        Console.WriteLine($"Found route from {key1} to {key2}, wihtout {without.Count} nodes");
        foreach (var p in path)
        {
            Console.WriteLine($"{p.from} -> {p.to}");
        }
        foreach (var edge in path)
        {
            without.Add(edge);
        }
        count++;
        if (count >= 4)
            break;
    }

    Console.WriteLine($"Counted routes, {key1} to {key2}, {count}");

    return count;
}

List<(int from, int to)> canReachWithoutEdges(int start, int end, HashSet<(int from, int to)> without)
{
    int maxDepth = 0;
    var queue = new Queue<(int, HashSet<int>, List<(int from, int to)>, int)>();
    queue.Enqueue((start, [start], new List<(int from, int to)>(), 0));
    while (queue.Any())
    {
        (var curr, var visited, var route, var depth) = queue.Dequeue();
        if (curr == end)
        {
            return route;
        }
        if (depth > maxDepth)
        {
            Console.WriteLine($"Max depth: {maxDepth}");
            maxDepth = depth;
        }

        if (depth > 12)
            return null;

        foreach (var edge in graph[curr])
        {
            (var from, var to) = (curr <= edge ? curr : edge, curr <= edge ? edge : curr);
            if (without.Contains((from, to)))
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

int countNodes(string start)
{
    HashSet<string> visited = new HashSet<string>();
    var queue = new Queue<string>();
    queue.Enqueue(start);
    while (queue.Any())
    {
        var curr = queue.Dequeue();
        visited.Add(curr);
        foreach (var edge in graphStr[curr])
        {
            if (visited.Contains(edge))
                continue;

            queue.Enqueue(edge);
        }
    }
    return visited.Count;
}
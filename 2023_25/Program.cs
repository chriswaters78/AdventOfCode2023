var graph = new Dictionary<string, HashSet<string>>();
foreach (var line in File.ReadAllLines("input.txt"))
{
    //jqt: rhn xhk nvd
    var sp = line.Split(": ");
    if (!graph.ContainsKey(sp[0]))
        graph[sp[0]] = new HashSet<string>();
    foreach (var edge in sp[1].Split(" "))
    {
        graph[sp[0]].Add(edge);
        if (!graph.ContainsKey(edge))
            graph[edge] = new HashSet<string>();
        graph[edge].Add(sp[0]);
    }
}

Console.WriteLine($"{graph.Keys.Count} nodes, {graph.Values.Sum(l => l.Count)} edges");

var key1 = graph.Keys.First();
var routeCounts = (from key2 in graph.Keys
                   where key2 != key1
                   select (key1, key2, countRoutes(key1, key2))).ToList();

var set1 = new HashSet<string>();
set1.Add(key1);
var set2 = new HashSet<string>();
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

int countRoutes(string key1, string key2)
{
    var without = new HashSet<(string from, string to)>();
    int count = 0;
    while (true)
    {
        var path = canReachWithoutEdges(key1, key2, without);
        if (path == null)
        {
            break;
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

List<(string from, string to)> canReachWithoutEdges(string start, string end, HashSet<(string from, string to)> without)
{
    var stack = new Stack<(string, HashSet<string>, List<(string from, string to)>)>();
    stack.Push((start, [start], new List<(string from, string to)>()));
    while (stack.Any())
    {
        (var curr, var visited, var route) = stack.Pop();
        if (curr == end)
        {
            return route;
        }

        foreach (var edge in graph[curr])
        {
            (var from, var to) = (curr.CompareTo(edge) == -1 ? curr : edge, curr.CompareTo(edge) == -1 ? edge : curr);
            if (without.Contains((from, to)))
                continue;

            if (visited.Contains(edge))
                continue;

            var newVisited = visited.ToHashSet();
            newVisited.Add(edge);
            var newRoute = route.ToList();
            newRoute.Add((from, to));
            stack.Push((edge, newVisited, newRoute));
        }
    }
    return null;
}

List<HashSet<string>> bfs(string start, string end)
{
    var results = new List<HashSet<string>>();
    var queue = new Queue<(string, HashSet<string>)>();
    queue.Enqueue((start, [start]));
    while (queue.Any())
    {
        (var curr, var visited) = queue.Dequeue();
        if (curr == end)
        {
            results.Add(visited);
            continue;
        }

        foreach (var edge in graph[curr])
        {
            if (visited.Contains(edge))
                continue;

            var newVisited = visited.ToHashSet();
            newVisited.Add(edge);
            queue.Enqueue((edge, newVisited));
        }
    }
    return results;
}
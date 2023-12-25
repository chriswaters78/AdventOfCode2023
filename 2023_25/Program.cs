var graph = new Dictionary<string, HashSet<string>>();
foreach (var line in File.ReadAllLines("test.txt"))
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

List<HashSet<(string from, string to)>> found = new List<HashSet<(string from, string to)>>();

var key1 = graph.Keys.First();
foreach (var key2 in graph.Keys.Skip(1).ToList())
{
    var without = new HashSet<(string from, string to)>();
    bool canReachViaFourRoutes = true;
    for (int i = 0; i < 3; i++)
    {
        var path = canReachWithoutEdges(key1, key2, without);
        if (path == null)
        {
            canReachViaFourRoutes = false;
            //we couldn't reach three times, these nodes are in the two sets
        }
        else
        {
            foreach (var edge in path)
            {
                without.Add(edge);
            }
        }
    }
    if (canReachViaFourRoutes)
    {
        //merge our two nodes to create a new graph
        var newKey = $"{key1}{key2}";
        graph[newKey] = new HashSet<string>();
        foreach (var edge in graph[key1])
        {
            if (edge == key2)
                continue;

            graph[newKey].Add(edge);
            graph[edge].Add(newKey);
        }
        foreach (var edge in graph[key2])
        {
            if (edge == key1)
                continue;

            if (edge != newKey)
                graph[newKey].Add(edge);

            graph[edge].Add(newKey);
        }
        graph.Remove(key1);
        graph.Remove(key2);

        foreach (var key in graph.Keys.ToList())
        {
            if (graph[key].Contains(key1))
            {
                graph[key].Remove(key1);
                graph[key].Add(newKey);
            }
            if (graph[key].Contains(key2))
            {
                graph[key].Remove(key2);
                graph[key].Add(newKey);
            }
        }

        key1 = newKey;
    }
}


Console.WriteLine();

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
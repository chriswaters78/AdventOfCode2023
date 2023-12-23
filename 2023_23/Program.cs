using System.Collections.Generic;
using System.Numerics;
var watch = new System.Diagnostics.Stopwatch();
watch.Start();
List<Complex> offsets = [new Complex(1, 0), new Complex(0, 1), new Complex(-1, 0), new Complex(0, -1)];

var grid = File.ReadAllLines("input.txt");

(int R, int C) = (grid.Length, grid.First().Length);

var allNodes = Enumerable.Range(0, R).SelectMany(r => Enumerable.Range(0, C).Where(
    c =>    grid[r][c] != '#' && new (int r, int c)[] { (r - 1, c), (r, c - 1), (r + 1, c), (r, c + 1) }.Where(
                tp => tp.r >= 0 && tp.c >= 0 && tp.r < R && tp.c < C
                && grid[tp.r][tp.c] != '#')
            .Count() >= 3).Select( c => (r,c))).ToHashSet();

var start = (0, grid.First().Select((ch, i) => (ch, i)).Where(tp => tp.ch == '.').Single().i);
var end = (grid.First().Length - 1, grid.Last().Select((ch, i) => (ch, i)).Where(tp => tp.ch == '.').Single().i);
allNodes.Add(start);
allNodes.Add(end);

Console.WriteLine($"start: {start}, end: {end}");
Console.WriteLine(String.Join(",", allNodes.Select(tp => $"({tp.r},{tp.c})")));
Console.WriteLine($"Node count: {allNodes.Count}");


var visited = new HashSet<(int r, int c)>();
var graph = edgeContraction(false);
Console.WriteLine($"Part1: {solve(start, 0)} in {watch.ElapsedMilliseconds}ms");

visited.Clear();
graph = edgeContraction(true);
Console.WriteLine($"Part2: {solve(start, 0)} in {watch.ElapsedMilliseconds}ms");


int solve((int r, int c) current, int length)
{
    if (current == end)
        return length;

    int max = int.MinValue;
    foreach (var n in graph[current])
    {
        if (!visited.Add(n.to))
            continue;

        max = Math.Max(solve(current = n.to, length + n.length), max);
        visited.Remove(n.to);
    }

    return max;
}

Dictionary<(int r, int c), List<Edge>> edgeContraction(bool ignoreSlopes)
{
    var graph = allNodes.ToDictionary(tp => tp, _ => new List<Edge>());
    foreach (var node in allNodes)
    {
        var queue = new Queue<State>();
        queue.Enqueue(new State(node, node, 0));
        while (queue.Count > 0)
        {
            var state = queue.Dequeue();

            if (state.current != node && allNodes.Contains(state.current))
            {
                graph[node].Add(new Edge(state.current, state.steps));
                continue;
            }

            var next = new[] { (state.current.r - 1, state.current.c), (state.current.r, state.current.c - 1),
            (state.current.r + 1, state.current.c),(state.current.r, state.current.c + 1)}
                //no backtracking, no going off grid
                .Where<(int r, int c)>(n => n != state.last && n.r >= 0 && n.r < R && n.c >= 0 && n.c < C)
                .Where(n => grid[n.Item1][n.Item2] switch
                {
                    '#' => false,
                    '.' => true,
                    'v' when n.r - state.current.r == 0 => throw new(),
                    'v' => ignoreSlopes || n.r - state.current.r > 0,
                    '>' when n.c - state.current.c == 0 => throw new(),
                    '>' => ignoreSlopes || n.c - state.current.c > 0,
                    '<' => throw new(),
                    '^' => throw new(),
                })
                .ToList();

            foreach (var n in next)
            {
                queue.Enqueue(new State(n, state.current, state.steps + 1));
            }
        }
    }
    return graph;
}

record struct Edge((int r, int c) to, int length);
record struct State ((int r, int c) current, (int r, int c) last, int steps);
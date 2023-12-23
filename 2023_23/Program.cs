using System.Numerics;
List<Complex> offsets = [new Complex(1, 0), new Complex(0, 1), new Complex(-1, 0), new Complex(0, -1)];

var grid = File.ReadAllLines("input.txt")
    .SelectMany((line, r) => line.Select((ch, c) => (ch, new Complex(r, c))))
    .ToDictionary(tp => tp.Item2, tp => tp.ch);

(int R, int C) = ((int) grid.Keys.Max(p => p.Real), (int) grid.Keys.Max(p => p.Imaginary));

var start = new Complex(0,Enumerable.Range(0, C).Where(c => grid[new Complex(0, c)] == '.').Single());
var end = new Complex(R, Enumerable.Range(0, C).Where(c => grid[new Complex(R, c)] == '.').Single());

var graph = buildGraph();
Console.WriteLine($"Graph nodes: {graph.Keys.Count}");

var queue = new Queue<GState>();
queue.Enqueue(new GState(start, [start], 0));

int maxLength = 0;
while (queue.Count > 0)
{
    var state = queue.Dequeue();

    if (state.curr == end)
    {
        //we are done, remove first two 'non steps' and add the final step onto the taken list
        if (state.length > maxLength)
        {
            maxLength = state.length;
            Console.WriteLine($"Max: {maxLength}");
        }
        continue;
    }

    foreach (var n in graph[state.curr])
    {
        //no backtracking
        if (state.visited.Contains(n.to)) 
            continue;

        var newVisited = state.visited.ToHashSet();
        newVisited.Add(n.to);
        queue.Enqueue(new GState(n.to, newVisited, state.length + n.length));
    }
}

Console.WriteLine($"Part2: {maxLength}");

Dictionary<Complex, HashSet<Edge>> buildGraph()
{
    var queue = new Queue<State>();
    queue.Enqueue(new State(start, new Complex(-1, 1), start, 0));
    var graph = new Dictionary<Complex, HashSet<Edge>>();
    graph.Add(start, new HashSet<Edge>());
    while (queue.Count > 0)
    {
        var curr = queue.Dequeue();

        if (curr.p == end)
        {
            graph[curr.p] = new HashSet<Edge>();
            graph[curr.p].Add(new Edge(curr.lastJunction, curr.stepsSinceLastJunction));
            graph[curr.lastJunction].Add(new Edge(curr.p, curr.stepsSinceLastJunction));
            continue;
        }

        var next = offsets.Select(os => os + curr.p)
            //no backtracking
            .Where(n => n != curr.lastPosition)
            .Where(n => grid[n] switch
            {
                '#' => false,
                _ => true,
            })
            .ToList();

        if (next.Count > 1)
        {
            //we have found a junction from lastJunction to here so add to the graph
            if (!graph.ContainsKey(curr.p))
            {
                graph[curr.p] = new HashSet<Edge>();
                foreach (var n in next)
                {
                    queue.Enqueue(new State(n, curr.p, curr.p,1));
                }
            }
            graph[curr.lastJunction].Add(new Edge(curr.p, curr.stepsSinceLastJunction));
            graph[curr.p].Add(new Edge(curr.lastJunction, curr.stepsSinceLastJunction));
        }
        else if (next.Count == 1)
        {
            queue.Enqueue(new State(next.Single(), curr.p, curr.lastJunction, curr.stepsSinceLastJunction + 1));
        }
    }
    return graph;
}

record struct Edge(Complex to, int length);

record struct GState(Complex curr, HashSet<Complex> visited, int length);
record struct State (Complex p, Complex lastPosition, Complex lastJunction, int stepsSinceLastJunction);
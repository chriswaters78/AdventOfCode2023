using System.Numerics;
List<Complex> offsets = [new Complex(1, 0), new Complex(0, 1), new Complex(-1, 0), new Complex(0, -1)];

var grid = File.ReadAllLines("input.txt")
    .SelectMany((line, r) => line.Select((ch, c) => (ch, new Complex(r, c))))
    .ToDictionary(tp => tp.Item2, tp => tp.ch);

(int R, int C) = ((int) grid.Keys.Max(p => p.Real), (int) grid.Keys.Max(p => p.Imaginary));

var start = new Complex(0,Enumerable.Range(0, C).Where(c => grid[new Complex(0, c)] == '.').Single());
var end = new Complex(R, Enumerable.Range(0, C).Where(c => grid[new Complex(R, c)] == '.').Single());


var queue = new Queue<State>();
queue.Enqueue(new State(new Complex(0, 1), [new Complex(-1,1)]));

var finished = new List<List<Complex>>();
while (queue.Count > 0)
{
    var curr = queue.Dequeue();

    if (curr.p == end)
    {
        //we are done, remove first two 'non steps' and add the final step onto the taken list
        finished.Add(curr.taken.Skip(2).Concat([curr.p]).ToList());
        continue;
    }

    var next = offsets.Select(os => os + curr.p)
        .Where(n => curr.p.Real >= 0 && curr.taken.Last() != n)        
        .Where(n => grid[n] switch
        {
            '#' => false,
            '.' => true,
            'v' when n.Real - curr.p.Real == 0 => throw new(),
            'v' => n.Real - curr.p.Real > 0,
            '>' when n.Imaginary - curr.p.Imaginary == 0 => throw new(),
            '>' => n.Imaginary - curr.p.Imaginary > 0,
            '<' => throw new(),
            '^' => throw new(),
        })
        .ToList();

    foreach (var n in next)
    {
        queue.Enqueue(new State(n, curr.taken.Concat([curr.p]).ToList()));
    }
}

Console.WriteLine($"Found {finished.Count} paths");
var part1 = finished.Max(taken => taken.Count);
Console.WriteLine($"Part1: {part1}");

record struct State (Complex p, List<Complex> taken);
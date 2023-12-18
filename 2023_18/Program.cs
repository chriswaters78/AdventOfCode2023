using System.Collections;
using System.Numerics;
using System.Text;

var lines = File.ReadAllLines("input.txt").Select(line => line.Split(" ")).Select(arr => new { Dir = arr[0][0], Move = int.Parse(arr[1]), Colour = arr[2].Trim('(', ')', '#') }).ToList();

var grid = new Dictionary<Complex, char>();

var current = new Complex(0, 0);
foreach (var  line in lines)
{
    for (int m = 0; m < line.Move; m++)
    {
        grid[current] = '#';
        current = current + move(line.Dir);
    }
}

(var R0, var R1) = (grid.Keys.Min(key => (int)key.Real) - 1, grid.Keys.Max(key => (int)key.Real) + 1);
(var C0, var C1) = (grid.Keys.Min(key => (int)key.Imaginary) - 1, grid.Keys.Max(key => (int)key.Imaginary) + 1);

Console.WriteLine(print(grid));

var queue = new Queue<Complex>();
queue.Enqueue(new Complex(R0, C0));
while (queue.Any())
{
    var curr = queue.Dequeue();

    foreach (var mv in new[] { move('R'), move('L'), move('D'), move('U') })
    {
        var next = curr + mv;
        if (next.Real < R0 || next.Real > R1 || next.Imaginary < C0 || next.Imaginary > C1)
        {
            continue;
        }
        if (grid.ContainsKey(next))
        {
            continue;
        }
        grid[next] = 'O';

        queue.Enqueue(next);
    }
}

Console.WriteLine(print(grid));

var part1 = Enumerable.Range(R0, R1 - R0).SelectMany(r => Enumerable.Range(C0, C1 - C0).Select(c =>
{
    var key = new Complex(r, c);
    if (!grid.ContainsKey(key) || grid[key] == '#')
        return 1;
    return 0;
})).Sum();

Console.WriteLine($"Part1: {part1}");


string print(Dictionary<Complex, char> grid)
{
    StringBuilder sb = new StringBuilder();
    for (int r = R0; r <= R1; r++)
    {
        for (int c = C0; c <= C1; c++)
        {
            var key = new Complex(r, c);
            sb.Append(grid.ContainsKey(key) ? grid[key] : '.');
        }
        sb.AppendLine();
    }

    return sb.ToString();
}

Complex move(char dir) => dir switch
{
    'R' => new Complex(0, 1),
    'D' => new Complex(1, 0),
    'L' => new Complex(0, -1),
    'U' => new Complex(-1, 0),
};

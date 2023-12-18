using System.Collections;
using System.Numerics;
using System.Text;

var lines = File.ReadAllLines("input.txt").Select(line => line.Split(" ")).Select(arr => (arr[0..2], arr[2].Trim('(', ')', '#'))).Select(
    tp => new { Dir1 = tp.Item1[0][0], Move1 = int.Parse(tp.Item1[1]), Dir2 = "RDLU"[int.Parse($"{tp.Item2.Last()}")], Move2 = int.Parse(tp.Item2[0..^1], System.Globalization.NumberStyles.HexNumber) }).ToList();

var grid = new Dictionary<Complex, char>();

var part1 = solve(lines.Select(a => (a.Dir1, a.Move1)));
Console.WriteLine($"Part1: {part1}");
var part2 = solve(lines.Select(a => (a.Dir2, a.Move2)));
Console.WriteLine($"Part2: {part2}");

long solve(IEnumerable<(char d, int m)> steps)
{
    var current = new Complex(0, 0);
    var vertices = new List<Complex>(new[] { current });
    foreach (var step in steps)
    {
        current = current + step.m * move(step.d);
        vertices.Add(current);
    }

    var answer = 0L;
    //run the shoelace algorithm
    for (int v = vertices.Count - 1 ; v > 0; v--)
    {
        answer += ((long)vertices[v - 1].Real + (long)vertices[v].Real) * ((long)vertices[v - 1].Imaginary - (long)vertices[v].Imaginary);
    }
    answer /= 2;

    //add the additional area within the border
    //we have half the path length, plus one for the 4 excess right turns
    answer += steps.Sum(tp => tp.m) / 2 + 1;

    return answer;
}

Complex move(char dir) => dir switch
{
    'R' => new Complex(0, 1),
    'D' => new Complex(1, 0),
    'L' => new Complex(0, -1),
    'U' => new Complex(-1, 0),
};

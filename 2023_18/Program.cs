using System.Numerics;

var moves = new [] {
        ( "R0", new Complex(0, 1) ),
        ( "D1", new Complex(1, 0) ),
        ( "L2", new Complex(0, -1) ),
        ( "U3", new Complex(-1, 0) ) }
    .SelectMany(tp => tp.Item1.Select(ch => (ch, tp.Item2)))
    .ToDictionary(tp => tp.ch, tp => tp.Item2);

var lines = File.ReadAllLines("input.txt").Select(line => line.Split(" ")).Select(arr => (arr[0..2], arr[2].Trim('(', ')', '#')));
var part1 = lines.Select(tp => (moves[tp.Item1[0][0]], int.Parse(tp.Item1[1])));
var part2 = lines.Select(tp => (moves[tp.Item2.Last()], int.Parse(tp.Item2[0..^1], System.Globalization.NumberStyles.HexNumber)));

Console.WriteLine($"Part1: {solve(part1)}");
Console.WriteLine($"Part2: {solve(part2)}");

long solve(IEnumerable<(Complex d, int m)> steps)
{
    var current = new Complex(0, 0);
    var vertices = new List<Complex>(new[] { current });
    foreach (var step in steps)
    {
        current = current + step.m * step.d;
        vertices.Add(current);
    }

    var A = 0L;
    //Shoelace algorithm, A = sum over all anti-clockwise vertices (y+1 + y)(x+1 - x)/2
    for (int v = 0; v <  vertices.Count - 1; v++)
    {
        A += ((long)vertices[v + 1].Real + (long)vertices[v].Real) * ((long)vertices[v + 1].Imaginary - (long)vertices[v].Imaginary);
    }
    //running clockwise gives -'ve area
    A /= -2;

    //Pick's: A = i + b/2 - 1
    //where A is area within a boundary running through midpoint of each # symbol
    //we are effectively using it on a half integer grid which works fine
    //i calculated from Pick's is then every interior point apart from the boundary
    //so then just add the length of the boundary as well
    var b = steps.Sum(tp => tp.m);
    var i = A - b / 2 + 1;
    return i + b;    
}


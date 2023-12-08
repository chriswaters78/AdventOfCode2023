using System.Numerics;

var file = File.ReadAllLines("input.txt");
var instructions = file[0];
var maps = file.Skip(2).Select(l => l.Split(" = ")).ToDictionary(
        sp => sp[0], 
        sp => sp[1].Trim('(', ')').Split(", ").ToArray());

Func<string, long> findSteps = current =>
{
    for (var i = 0; i < int.MaxValue; i++)
    {
        current = maps[current][instructions[i % instructions.Length] == 'L' ? 0 : 1];
        if (current.EndsWith('Z'))
        {
            return i + 1;
        }
    }
    throw new();
};

var steps = maps.Keys
    .Where(key => key.EndsWith('A'))
    .Select(key => (key, findSteps(key)))
    .ToDictionary(tp => tp.key, tp => tp.Item2);

Console.WriteLine($"Part1: {steps["AAA"]}");
var part2 = steps.Values.Aggregate(BigInteger.One, (a, b) => a * b / BigInteger.GreatestCommonDivisor(a,b));
Console.WriteLine($"Part2: {part2}");

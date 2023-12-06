var file = File.ReadAllLines("input.txt")
    .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries)
    .Skip(1).Select(int.Parse).ToList()).ToList();

var races = file[0].Zip(file[1]).Select(tp =>
{
    var time = tp.First;
    return Enumerable.Range(0, time + 1).Count(t => (t * (time - t)) > tp.Second);
});

var part1 = races.Aggregate(1, (acc, val) => acc * val);
Console.WriteLine($"Part1: {part1}");

int part2 = 0;
long T = 56977793;
long D = 499221010971440;
for (long time = 0; time < T; time++)
{
    if (time * (T - time) > D)
    {
        part2++;
    }
}
Console.WriteLine($"Part2: {part2}");

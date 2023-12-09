int part1 = 0;
int part2 = 0;
foreach (var line in File.ReadAllLines("input.txt"))
{
    var sequences = new List<List<int>>();
    sequences.Add(line.Split(" ").Select(int.Parse).ToList());
    while (sequences.Last().Any(v => v != 0))
    {
        var newSequence = new List<int>();
        for (int i = 1; i < sequences.Last().Count; i++)
        {
            newSequence.Add(sequences.Last()[i] - sequences.Last()[i - 1]);
        }
        sequences.Add(newSequence);
    }
    sequences.Last().Add(0);
    sequences.Last().Insert(0, 0);
    for (int s = sequences.Count - 2; s >= 0; s--)
    {
        sequences[s].Add(sequences[s].Last() + sequences[s + 1].Last());
        sequences[s].Insert(0, sequences[s].First() - sequences[s + 1].First());
    }
    part1 += sequences[0].Last();
    part2 += sequences[0].First();
}

Console.WriteLine($"Part1: {part1}");
Console.WriteLine($"Part2: {part2}");

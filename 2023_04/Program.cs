var part1 = 0;
var file = File.ReadAllLines("input.txt");
var cardCounts = Enumerable.Range(0, file.Length).ToDictionary(i => i, _ => 1);
foreach (var line in file.Select((str,i) => (str,i)))
{
    var sp = line.str.Split(" | ");
    var card1 = sp[0].Split(": ")[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str.Trim())).ToList();
    var card2 = sp[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str.Trim())).ToList();

    var matches = card1.Intersect(card2).ToList();
    var score = (int) Math.Pow(2, matches.Count - 1);

    for (int wonCards = line.i + 1; wonCards < line.i + 1 + matches.Count; wonCards++)
    {
        cardCounts[wonCards] += cardCounts[line.i];
    }

    part1 += score;
}

Console.WriteLine($"Part1: {part1}");
Console.WriteLine($"Part2: {cardCounts.Values.Sum()}");

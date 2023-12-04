namespace _2023_04
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var part1 = 0;
            var file = File.ReadAllLines("input.txt");
            var cardCounts = Enumerable.Range(1, file.Length).ToDictionary(i => i, _ => 1);
            int card = 1;
            foreach (var line in file)
            {
                var sp = line.Split(" | ");
                var card1 = sp[0].Split(": ")[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str.Trim())).ToList();
                var card2 = sp[1].Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str.Trim())).ToList();

                var matches = card1.Intersect(card2).ToList();
                var score = matches.Count > 0 ? (int) (Math.Pow(2, matches.Count - 1)) : 0;

                for (int wonCards = card + 1; wonCards < card + 1 + matches.Count; wonCards++)
                {
                    cardCounts[wonCards] += cardCounts[card];
                }

                part1 += score;
                card++;
            }

            Console.WriteLine($"Part1: {part1}");
            var part2 = cardCounts.Values.Sum();
            Console.WriteLine($"Part2: {part2}");
        }
    }
}
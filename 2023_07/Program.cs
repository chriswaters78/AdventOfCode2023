namespace _2023_07
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hands = File.ReadAllLines("input.txt").Select(line =>
            {
                var sp = line.Split(" ");
                return new { Hand = parseHand(sp[0]), Hand2 = parseHand2(sp[0]), Type = getType(parseHand(sp[0])), Type2 = getType2(parseHand2(sp[0])), Value = int.Parse(sp[1]) };
            }).ToList();

            var part1 = hands.OrderBy(h => h.Type).ThenBy(h => h.Hand).Select((tp, rank) => (long) (tp.Value * (rank + 1))).Sum();
            Console.WriteLine($"Part1: {part1}");
            var part2 = hands.OrderBy(h => h.Type2).ThenBy(h => h.Hand2).Select((tp, rank) => (long)(tp.Value * (rank + 1))).Sum();
            Console.WriteLine($"Part2: {part2}");
        }

        private static string parseHand(string hand)
        {
            return hand.Replace("J", "B").Replace("Q", "C").Replace("K", "D").Replace("A", "E").Replace("T", "A");
        }
        private static string parseHand2(string hand)
        {
            return hand.Replace("J", "1").Replace("Q", "C").Replace("K", "D").Replace("A", "E").Replace("T", "A");
        }
        private static int getType2(string hand)
        {
            int max = -1;
            foreach (var ch in "23456789ABCDE")
            {
                var type = getType(hand.Replace('1', ch));
                max = Math.Max(max, type);
            }
            return max;
        }

        private static int getType(string hand)
        {
            var grouped = hand.GroupBy(ch => ch).OrderByDescending(grp => grp.Count()).ToList();
            if (grouped.Count() == 1)
                return 6;
            if (grouped.Count() == 2 && grouped.First().Count() == 4)
                return 5;
            if (grouped.Count == 2)
                return 4;
            if (grouped.First().Count() == 3)
                return 3;
            if (grouped.First().Count() == 2 && grouped.Skip(1).First().Count() == 2)
                return 2;
            if (grouped.First().Count() == 2)
                return 1;
            return 0;
        }
    }
}
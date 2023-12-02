namespace _2023_02
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var lines = File.ReadAllLines("input.txt");

            var part1 = 0;
            var part2 = 0;
            foreach (var line in lines)
            {
                var sp = line.Split(":");
                var gameNo = int.Parse(sp[0].Split(" ").Last());
                var games = sp[1].Trim().Split("; ").Select(game => game.Split(",").Select(colour =>
                    {
                        var sp2 = colour.Trim().Split(" ");
                        return new { Colour = sp2[1], Number = int.Parse(sp2[0]) };
                    }).ToList()
                ).ToList();

                var maxBlue = games.SelectMany(a => a).Where(a => a.Colour == "blue").Select(a => a.Number).Concat(new[] { 0 }).Max();
                var maxGreen = games.SelectMany(a => a).Where(a => a.Colour == "green").Select(a => a.Number).Concat(new[] { 0 }).Max();
                var maxRed = games.SelectMany(a => a).Where(a => a.Colour == "red").Select(a => a.Number).Concat(new[] { 0 }).Max();

                if (maxBlue <= 14 && maxGreen <= 13 && maxRed <= 12)
                {
                    part1 += gameNo;
                }

                var power = maxBlue * maxGreen * maxRed;
                part2 += power;


            }

            Console.WriteLine($"Part1: {part1}");
            Console.WriteLine($"Part2: {part2}");
        }
    }
}
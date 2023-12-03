namespace _2023_03
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var offsets = new (int x, int y)[] { (-1,0),(0,-1), (1,0), (0,1), (-1,-1),(-1,1),(1,-1),(1,1) };
            
            var lines = File.ReadAllLines("input.txt");
            
            var gears = new Dictionary<(int x, int y), List<int>>();
            int part1 = 0;
            for (int y = 0; y < lines.Length; y++)
            {
                string number = "";
                bool adjacent = false;
                var line = lines[y];
                var adjacentStars = new HashSet<(int x, int y)>();
                for (int x = 0; x <= line.Length; x++)
                {
                    if (x < line.Length && Char.IsDigit(line[x]))
                    {
                        number = $"{number}{line[x]}";
                        foreach (var offset in offsets)
                        {
                            var ox = offset.x + x;
                            var oy = offset.y + y;
                            if (ox < 0 || oy < 0 || ox >= line.Length || oy >= lines.Length)
                            {
                                continue;
                            }
                            if (!Char.IsDigit(lines[oy][ox]) && lines[oy][ox] != '.')
                            {
                                adjacent = true;
                                if (lines[oy][ox] == '*')
                                {
                                    adjacentStars.Add((ox, oy));
                                }
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (adjacent)
                        {
                            part1 += int.Parse(number);
                            foreach (var star in adjacentStars)
                            {
                                if (!gears.ContainsKey(star))
                                {
                                    gears.Add(star, new List<int>());
                                }
                                gears[star].Add(int.Parse(number));
                            }
                            
                        }
                        number = "";
                        adjacent = false;
                        adjacentStars.Clear();
                    }
                }
            }
            
            Console.WriteLine($"Part1: {part1}");

            var part2 = gears.Where(kvp => kvp.Value.Count == 2).Sum(kvp => kvp.Value[0] * kvp.Value[1]);
            Console.WriteLine($"Part2: {part2}");
        }
    }
}
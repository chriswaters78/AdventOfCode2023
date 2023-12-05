namespace _2023_05
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var lines = File.ReadAllLines("input.txt");
            var seeds = lines[0].Split(": ")[1].Split(" ").Select(long.Parse).ToList();
            var pairedSeeds = seeds.Select((seed,i) => (seed,i)).GroupBy(tp => tp.i / 2).Select(grp => (grp.First().seed,grp.Skip(1).First().seed)).ToList();
            var totalPaired = pairedSeeds.Sum(tp => tp.Item2);
            var maps = Enumerable.Range(0, 7).Select(_ => new List<(long destination, long source, long length)>()).ToArray();
            int current = -1;
            foreach (var line in lines.Skip(1))
            {
                if (String.IsNullOrEmpty(line)) continue;
                if (!Char.IsDigit(line[0]))
                {
                    current++;
                    continue;
                }

                var sp = line.Split(" ").Select(long.Parse).ToArray();
                maps[current].Add((sp[0], sp[1], sp[2]));
            }
            Func<long,long> map = initial =>
            {
                var mapped = initial;
                foreach (var map in maps)
                {
                    foreach (var mapping in map)
                    {
                        if (mapped >= mapping.source && mapped < mapping.source + mapping.length)
                        {
                            mapped = mapping.destination + (mapped - mapping.source);
                            break;
                        }
                    }
                }
                return mapped;
            };

            var soils = seeds.Select(map);

            var part1 = soils.Min();
            Console.WriteLine($"Part1: {part1}");

            var part2Soils = pairedSeeds.SelectMany(tp => range(tp.Item1, tp.Item2))
                .Select(map)
                .Min();

            Console.WriteLine($"Part2: {part2Soils}");
        }

        private static IEnumerable<long> range(long start, long count)
        {
            for (long l = start; l < start + count; l++)
            {
                yield return l;
            }
        }
    }
}
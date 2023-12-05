using System;
using System.Diagnostics;
using System.IO;

namespace _2023_05
{
    internal class Program
    {
        static List<(long destination, long source, long length)>[] maps;
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var lines = File.ReadAllLines("input.txt");
            var seeds = lines[0].Split(": ")[1].Split(" ").Select(long.Parse).ToList();
            var pairedSeeds = seeds.Select((seed,i) => (seed,i)).GroupBy(tp => tp.i / 2).Select(grp => (grp.First().seed,grp.Skip(1).First().seed)).ToList();
            var totalPaired = pairedSeeds.Sum(tp => tp.Item2);
            maps = Enumerable.Range(0, 7).Select(_ => new List<(long destination, long source, long length)>()).ToArray();
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

            var part1 = long.MaxValue;
            foreach (var seed in seeds)
            {
                var range = new List<(long start, long end)>() { (seed, seed + 1) };
                part1 = Math.Min(part1, solve(range));
            }
            Console.WriteLine($"Part1: {part1} in {stopwatch.ElapsedMilliseconds}ms");

            var part2 = long.MaxValue;
            foreach (var tp in pairedSeeds)
            {
                var range = new List<(long start, long end)>() { (tp.Item1, tp.Item1 + tp.Item2) };
                part2 = Math.Min(part2, solve(range));
            }

            Console.WriteLine($"Part2: {part2} in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static long solve(List<(long start, long end)> source)
        {
            var range = source;
            for (int mapIndex = 0; mapIndex < maps.Length; mapIndex++)
            {
                range = map2(mapIndex, range);
            }
            return range.Min(tp => tp.start);
        }
        private static List<(long start, long end)> map2(int mapIndex, List<(long start, long end)> source)
        {
            var destRanges = new List<(long start, long end)>();
            foreach (var (rangeStart, rangeEnd) in source)
            {
                foreach (var (ss, se, ds, de) in maps[mapIndex]
                    .Select(map => (map.source,map.source + map.length, map.destination, map.destination + map.length)))
                {
                    //if overlaps, add result, find remaining ranges and recursively call
                    // R|-----|
                    // M   |----|

                    // R    |-----|
                    // M |----|

                    //get any unmapped start
                    (long start, long end) us = (rangeStart, Math.Max(rangeStart, ss));
                    //get any mapped middle
                    (long start, long end) mm = (Math.Max(rangeStart, ss), Math.Min(rangeEnd, se));
                    //get any unmapped end
                    (long start, long end) ue = (Math.Max(rangeEnd, se), rangeEnd);

                    if (mm.start < mm.end)
                    {
                        //this map intersected this range, so recurse and add the mapped results
                        //we are done with this range now
                        destRanges.AddRange(new List<(long start, long end)>() { (ds + mm.start - ss, ds + mm.end - ss)});
                        if (us.start < us.end)
                        {
                            destRanges.AddRange(map2(mapIndex, new List<(long start, long end)>() { us }));
                        }
                        if (ue.start < ue.end)
                        {
                            destRanges.AddRange(map2(mapIndex, new List<(long start, long end)>() { ue }));
                        }
                        goto mapped;
                    }
                }
                //we didn't find any mappings, so whole range is unmapped
                destRanges.Add((rangeStart, rangeEnd));

            mapped:
                continue;
            }

            return destRanges;
        }
    }
}
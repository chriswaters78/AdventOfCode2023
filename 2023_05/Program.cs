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

            var part1 = seeds
                .Select(seed => solve(new List<(long start, long end)>() { (seed, seed + 1) }))
                .Min();

            Console.WriteLine($"Part1: {part1} in {stopwatch.ElapsedMilliseconds}ms");

            var part2 = seeds
                .Select((seed, i) => (seed, i)).GroupBy(tp => tp.i / 2)
                .Select(grp => (grp.First().seed, grp.Skip(1).First().seed))
                .Select(pair => solve(new List<(long start, long end)>() { (pair.Item1, pair.Item1 + pair.Item2) }))
                .Min();

            Console.WriteLine($"Part2: {part2} in {stopwatch.ElapsedMilliseconds}ms");
        }

        private static long solve(List<(long start, long end)> source) => maps.Aggregate(source, (acc, map) => Program.map(map, acc), acc => acc.Min(tp => tp.start));
        private static List<(long start, long end)> map(List<(long destination, long source, long length)> map, List<(long start, long end)> source)
        {
            var destRanges = new List<(long start, long end)>();
            foreach (var (rangeStart, rangeEnd) in source)
            {
                foreach (var (ss, se, ds, de) in map
                    .Select(map => (map.source,map.source + map.length, map.destination, map.destination + map.length)))
                {
                    //if overlaps, add result, find remaining ranges and recursively call
                    // R|-----|
                    // M   |----|

                    // R    |-----|
                    // M |----|

                    (long start, long end) unmappedStart = (rangeStart, Math.Max(rangeStart, ss));
                    (long start, long end) mappedMiddle = (Math.Max(rangeStart, ss), Math.Min(rangeEnd, se));
                    (long start, long end) unmappedEnd = (Math.Max(rangeEnd, se), rangeEnd);

                    if (mappedMiddle.start < mappedMiddle.end)
                    {
                        //this map intersected this rang
                        //add the mapped results
                        destRanges.AddRange(new List<(long start, long end)>() { (ds + mappedMiddle.start - ss, ds + mappedMiddle.end - ss)});
                        //and recurse over any unmapped sections
                        if (unmappedStart.start < unmappedStart.end)
                        {
                            destRanges.AddRange(Program.map(map, new List<(long start, long end)>() { unmappedStart }));
                        }
                        if (unmappedEnd.start < unmappedEnd.end)
                        {
                            destRanges.AddRange(Program.map(map, new List<(long start, long end)>() { unmappedEnd }));
                        }
                        goto mapped;
                    }
                }
                //we didn't find any mappings, so the whole range is unmapped
                destRanges.Add((rangeStart, rangeEnd));
            mapped:
                continue;
            }

            return destRanges;
        }
    }
}
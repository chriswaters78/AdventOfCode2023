var stopwatch = new System.Diagnostics.Stopwatch();
stopwatch.Start();

var lines = File.ReadAllLines("input.txt");
var seeds = lines[0].Split(": ")[1].Split(" ").Select(long.Parse).ToList();
var maps = Enumerable.Range(0, 7).Select(_ => new List<Map>()).ToArray();
int current = -1;
foreach (var line in lines.Skip(1).Where(str => !String.IsNullOrEmpty(str)))
{
    if (!Char.IsDigit(line[0]))
    {
        current++;
        continue;
    }

    var sp = line.Split(" ").Select(long.Parse).ToArray();
    maps[current].Add(new (sp[0], sp[1], sp[2]));
}

var part1 = seeds
    .Select(seed => solve(maps, new List<Range>() { new (seed, seed + 1) }))
    .Min();

Console.WriteLine($"Part1: {part1} in {stopwatch.ElapsedMilliseconds}ms");

var part2 = seeds
    .Select((seed, i) => (seed, i))
    .GroupBy(tp => tp.i / 2)
    .Select(grp => (grp.First().seed, grp.Skip(1).First().seed))
    .Select(pair => solve(maps, new List<Range>() { new (pair.Item1, pair.Item1 + pair.Item2) }))
    .Min();

Console.WriteLine($"Part2: {part2} in {stopwatch.ElapsedMilliseconds}ms");

static long solve(List<Map>[] maps, List<Range> source) => maps.Aggregate(source, (acc, m) => map(m, acc), acc => acc.Min(tp => tp.start));
static List<Range> map(List<Map> maps, List<Range> source)
{
    var destRanges = new List<Range>();
    foreach (var (rangeStart, rangeEnd) in source)
    {
        foreach (var (mapDestination, mapSource, mapLength) in maps)
        {
            // R|-----|
            // M   |----|

            // R    |-----|
            // M |----|

            //if overlaps, add result, find remaining ranges and recursively call
            var unmapped = new Range[] { new(rangeStart, Math.Max(rangeStart, mapSource)), new(Math.Max(rangeEnd, mapSource + mapLength), rangeEnd) };
            var mappedMiddle = new Range(Math.Max(rangeStart, mapSource), Math.Min(rangeEnd, mapSource + mapLength));

            if (mappedMiddle.start < mappedMiddle.end)
            {
                //this map intersected this range so add the mapped results
                destRanges.AddRange(new List<Range>() { new(mapDestination + mappedMiddle.start - mapSource, mapDestination + mappedMiddle.end - mapSource) });
                //and recurse over any unmapped sections
                destRanges.AddRange(unmapped.Where(range => range.start < range.end).SelectMany(range => map(maps, new List<Range>() { range })));
                goto mapped;
            }
        }
        //we didn't find any mappings, so the whole range is unmapped
        destRanges.Add(new (rangeStart, rangeEnd));
        mapped:;
    }
    return destRanges;
}

record struct Map(long destination, long source, long length);
record struct Range(long start, long end);

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

var part1 = seeds.Select(seed => solve(maps, new List<Range>() { new (seed, seed + 1) })).Min();
Console.WriteLine($"Part1: {part1} in {stopwatch.ElapsedMilliseconds}ms");

var part2 = seeds
    .Select((seed, i) => (seed, i))
    .GroupBy(tp => tp.i / 2, (key, grp) => solve(maps, 
        new Range[] { new (grp.First().seed, grp.First().seed + grp.Skip(1).First().seed) }))
    .Min();
Console.WriteLine($"Part2: {part2} in {stopwatch.ElapsedMilliseconds}ms");

static long solve(List<Map>[] maps, IEnumerable<Range> source) => maps.Aggregate(source, (acc, m) => map(m, acc), acc => acc.Min(tp => tp.start));
static List<Range> map(List<Map> maps, IEnumerable<Range> source)
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
            var mapped = new Range(Math.Max(rangeStart, mapSource), Math.Min(rangeEnd, mapSource + mapLength));
            if (mapped.start < mapped.end)
            {
                //this map intersected this range so add the mapped section as a result
                destRanges.AddRange(new Range[] { new(mapDestination + mapped.start - mapSource, mapDestination + mapped.end - mapSource) });
                //and for any unmapped splits, recurse and add their results
                //*** seems that none of these splits actually contribute to the result for my input ***
                destRanges.AddRange(
                    new Range[] { 
                        new(rangeStart, Math.Min(rangeEnd, mapSource)), 
                        new(Math.Max(rangeStart, mapSource + mapLength), rangeEnd) }
                    .Where(range => range.start < range.end)
                    .SelectMany(range => map(maps, new Range[] { range })));
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

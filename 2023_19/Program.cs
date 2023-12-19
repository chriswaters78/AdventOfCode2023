var lines = File.ReadAllText("input.txt").Split($"{Environment.NewLine}{Environment.NewLine}").ToArray();
var mappings = "xmas".Select((ch, i) => (ch, i)).ToDictionary(tp => tp.ch, tp => tp.i);

//px{a<2006:qkq, m>2090:A, rfg}
var rules = (from line in lines[0].Split($"{Environment.NewLine}")
             let sp = line.Trim('}').Split("{")
             select (sp[0], sp[1].Split(",").Select(pattern => pattern.Split(":") 
                 switch {
                    [var clause, var next] => (mappings[clause[0]], clause[1], int.Parse(clause[2..]), next),
                    [var next] => (-1, '_', 0, next),
                 }).ToList())).ToDictionary(tp => tp.Item1, tp => tp.Item2);

//{ x = 787,m = 2655,a = 1222,s = 2876}
var inputs = lines[1].Split($"{Environment.NewLine}").Select(line => line.Trim('{', '}').Split(",")).Select(
    arr => arr.Select(str => int.Parse(str[2..])).ToArray()).ToList();

var part1 = inputs.Select(item => new ItemRange("in", new[,] { { item[0], item[0] }, { item[1], item[1] }, { item[2], item[2] }, { item[3], item[3] } }))
    .SelectMany(solve)
    .Sum(ir => Enumerable.Range(0,4).Sum(i => ir.ranges[i,0]));
Console.WriteLine($"Part1: {part1}");

var part2 = solve(new ItemRange("in", new[,] { { 1, 4000 }, { 1, 4000 }, { 1, 4000 }, { 1, 4000 } }))
    .Sum(ir => Enumerable.Range(0, 4).Aggregate(1L, (acc, i) => acc * (ir.ranges[i, 1] - ir.ranges[i, 0] + 1)));

Console.WriteLine($"Part2: {part2}");

List<ItemRange> solve(ItemRange initial)
{
    var stack = new Stack<ItemRange>();
    var accepted = new List<ItemRange>();
    stack.Push(initial);
    while (stack.Any())
    {
        switch (stack.Pop())
        {
            case var curr when isEmpty(curr):
                continue;
            case var curr when curr.key == "A":
                accepted.Add(curr);
                continue;
            case var curr when curr.key == "R":
                continue;
            case var curr:
                var patterns = rules[curr.key];
                foreach ((int p, char o, int v, string next) in patterns)
                {
                    switch (o)
                    {
                        //eg x < 1500
                        //maxx is 1499
                        case '<':
                            stack.Push(cloneWithNewLimits(curr, next, p, 1, v - 1));
                            curr = cloneWithNewLimits(curr, curr.key, p, v, 4000);
                            break;
                        //eg x > 1500
                        //minx is 1501
                        case '>':
                            stack.Push(cloneWithNewLimits(curr, next, p, v + 1, 4000));
                            curr = cloneWithNewLimits(curr, curr.key, p, 1, v);
                            break;
                        case '_':
                            stack.Push(curr with { key = next });
                            break;
                    }
                }
                break;

        }
    }

    return accepted;
}

ItemRange cloneWithNewLimits(ItemRange ir, string key, int p, int min, int max)
{
    var newIr = ir with { key = key, ranges = (int[,]) ir.ranges.Clone() };
    newIr.ranges[p, 0] = Math.Max(ir.ranges[p, 0], min);
    newIr.ranges[p, 1] = Math.Min(ir.ranges[p, 1], max);
    return newIr;
};

bool isEmpty(ItemRange ir) => Enumerable.Range(0, 4).Any(i => ir.ranges[i, 1] < ir.ranges[i, 0]);

record struct ItemRange(string key, int[,] ranges);

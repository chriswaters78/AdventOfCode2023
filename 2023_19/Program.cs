var lines = File.ReadAllText("input.txt").Split($"{Environment.NewLine}{Environment.NewLine}").ToArray();

//px{a<2006:qkq, m>2090:A, rfg}
//{ x = 787,m = 2655,a = 1222,s = 2876}
var rules = lines[0].Split($"{Environment.NewLine}").Select(line =>
{
    var sp = line.Trim('}').Split("{");
    var key = sp[0];
    var patterns = new List<(char p, char o, int v, string next)>();
    foreach (var pattern in sp[1].Split(","))
    {
        if (pattern.Contains(":"))
        {
            var sp2 = pattern.Split(":");
            patterns.Add((sp2[0][0], sp2[0][1], int.Parse(sp2[0][2..]), sp2[1]));
        }
        else
        {
            patterns.Add(('_', '_', 0, pattern));
        }
    }

    return (key, patterns);
}).ToDictionary(tp => tp.key, tp => tp.patterns);

var inputs = lines[1].Split($"{Environment.NewLine}").Select(line => line.Trim('{').Trim('}').Split(",")).Select(
    arr => new Item(int.Parse(arr[0][2..]), int.Parse(arr[1][2..]), int.Parse(arr[2][2..]), int.Parse(arr[3][2..])));

var distinct = rules.SelectMany(tp => tp.Value.Where(tp => tp.o != '_').Select(tp => (tp.p, tp.o == '<' ? tp.v - 1 : tp.v + 1)))
    .GroupBy(tp => tp.p)
    .ToDictionary(grp => grp.Key, grp => grp.Select(tp => tp.Item2).ToList());

var part1Accepted = inputs.Select(item => new ItemRange("in", item.x, item.x, item.m, item.m, item.a, item.a, item.s, item.s)).SelectMany(solve).ToList();
var part1 = part1Accepted.Sum(ir => ir.minx + ir.minm + ir.mina + ir.mins);

Console.WriteLine($"Part1: {part1}");

var part2Accepted = solve(new ItemRange("in", 1, 4000, 1, 4000, 1, 4000, 1, 4000));
var part2 = part2Accepted.Sum(part2Sum);

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
            case var curr when curr.key == "A":
                if (hasAny(curr))
                {
                    accepted.Add(curr);
                }
                continue;
            case var curr when curr.key == "R":
                continue;
            case var curr:
                var patterns = rules[curr.key];
                foreach (var pattern in patterns)
                {
                    switch (pattern.o)
                    {
                        //eg x < 1500
                        //maxx is 1499
                        case '<':
                            stack.Push(setValue(curr, pattern.next, pattern.p, 1, pattern.v - 1));
                            curr = setValue(curr, curr.key, pattern.p, pattern.v, 4000);
                            break;
                        //eg x > 1500
                        //minx is 1501
                        case '>':
                            stack.Push(setValue(curr, pattern.next, pattern.p, pattern.v + 1, 4000));
                            curr = setValue(curr, curr.key, pattern.p, 1, pattern.v);
                            break;
                        case '_':
                            stack.Push(curr with { key = pattern.next });
                            break;
                    }
                }
                break;

        }
    }

    return accepted;
}

ItemRange setValue(ItemRange ir, string key, char p, int min, int max) => p switch
{
    'x' => ir with { key = key, minx = Math.Max(ir.minx, min), maxx = Math.Min(ir.maxx, max) },
    'm' => ir with { key = key, minm = Math.Max(ir.minm, min), maxm = Math.Min(ir.maxm, max) },
    'a' => ir with { key = key, mina = Math.Max(ir.mina, min), maxa = Math.Min(ir.maxa, max) },
    's' => ir with { key = key, mins = Math.Max(ir.mins, min), maxs = Math.Min(ir.maxs, max) },
};

bool hasAny(ItemRange ir)
{
    var values = getValues(ir);
    return values.x > 0 && values.m > 0 && values.a > 0 && values.s > 0;
}

long part2Sum(ItemRange ir)
{
    var values = getValues(ir);
    return values.x * values.m * values.a * values.s;
}

(long x, long m, long a, long s) getValues (ItemRange ir) => (ir.maxx - ir.minx + 1, ir.maxm - ir.minm + 1, ir.maxa - ir.mina + 1, ir.maxs - ir.mins + 1);

record struct Item (int x, int m, int a, int s);
record struct ItemRange(string key, int minx, int maxx, int minm, int maxm, int mina, int maxa, int mins, int maxs);



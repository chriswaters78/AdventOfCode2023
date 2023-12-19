//px{a<2006:qkq, m>2090:A, rfg}

//{ x = 787,m = 2655,a = 1222,s = 2876}

using System.Runtime.InteropServices;

var lines = File.ReadAllText("input.txt").Split($"{Environment.NewLine}{Environment.NewLine}").ToArray();

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

Console.WriteLine($"Part1: {part1()}");
Console.WriteLine($"Part2: {part2()}");

long part2()
{
    var itemRange = new ItemRange("in", 1, 4000, 1, 4000, 1, 4000, 1, 4000);

    var stack = new Stack<ItemRange>();
    Dictionary<ItemRange, bool> accepted = new Dictionary<ItemRange, bool>();
    stack.Push(itemRange);
    while (stack.Any())
    {
        var curr = stack.Pop();

        if (curr.key == "A" || curr.key == "R")
        {
            accepted.Add(curr, curr.key == "A");
            continue;
        }

        var patterns = rules[curr.key];
        foreach (var pattern in patterns)
        {
            switch (pattern.o)
            {
                //eg x < 1500
                //maxx is 1499
                case '<':
                    var next1 = setValue(curr, pattern.next, pattern.p, 1, pattern.v - 1);
                    stack.Push(next1);
                    curr = setValue(curr, curr.key, pattern.p, pattern.v, 4000);
                    break;
                //eg x > 1500
                //minx is 1501
                case '>':
                    var next2 = setValue(curr, pattern.next, pattern.p, pattern.v + 1, 4000);
                    stack.Push(next2);
                    curr = setValue(curr, curr.key, pattern.p, 1, pattern.v);
                    break;
                case '_':
                    stack.Push(curr with { key = pattern.next });
                    goto done;
            }
        }
    done:;
    }

    long part2 = 0;
    foreach (var ir in accepted.Where(kvp => kvp.Value))
    {
        long x = (ir.Key.maxx - ir.Key.minx + 1);
        long m = (ir.Key.maxm - ir.Key.minm + 1);
        long a = (ir.Key.maxa - ir.Key.mina + 1);
        long s = (ir.Key.maxs - ir.Key.mins + 1);
        if (x > 0 && m > 0 && a > 0 && s > 0)
        {
            part2 += x * m * a * s;
        }
    }

    return part2;
}

Console.WriteLine($"Part2: {part2}");

long part1()
{
    var accepted = new HashSet<Item>();
    foreach (var input in inputs)
    {
        string next = "in";
        while (true)
        {
            var curr = rules[next];
            foreach (var pattern in curr)
            {
                if (pattern.o == '_')
                {
                    next = pattern.next;
                    break;
                }

                var pv = pattern.p switch
                {
                    'x' => input.x,
                    'm' => input.m,
                    'a' => input.a,
                    's' => input.s,
                };

                switch (pattern.o)
                {
                    case '<':
                        if (pv < pattern.v)
                        {
                            next = pattern.next;
                            goto done;
                        }
                        break;
                    case '>':
                        if (pv > pattern.v)
                        {
                            next = pattern.next;
                            goto done;
                        }
                        break;
                }
            }
        done:;

            if (next == "A")
            {
                accepted.Add(input);
                break;
            }
            else if (next == "R")
            {
                break;
            }
        }
    }
    return accepted.Sum(item => item.x + item.m + item.a + item.s);
}

ItemRange setValue(ItemRange ir, string key, char p, int min, int max) => p switch
{
    'x' => ir with { key = key, minx = Math.Max(ir.minx, min), maxx = Math.Min(ir.maxx, max) },
    'm' => ir with { key = key, minm = Math.Max(ir.minm, min), maxm = Math.Min(ir.maxm, max) },
    'a' => ir with { key = key, mina = Math.Max(ir.mina, min), maxa = Math.Min(ir.maxa, max) },
    's' => ir with { key = key, mins = Math.Max(ir.mins, min), maxs = Math.Min(ir.maxs, max) },
};

record struct Item (int x, int m, int a, int s);
record struct ItemRange(string key, int minx, int maxx, int minm, int maxm, int mina, int maxa, int mins, int maxs);



//px{a<2006:qkq, m>2090:A, rfg}

//{ x = 787,m = 2655,a = 1222,s = 2876}

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

int part1 = 0;
foreach (var item in accepted)
{
    part1 += item.x + item.m + item.a + item.s;
}

Console.WriteLine($"Part1: {part1}");

//foreach (var input in inputs)
//{
//    Console.WriteLine($"{input.x}:{input.m}:{input.a}:{input.s}");
//}
//foreach (var (key, patterns) in rules)
//{
//    Console.WriteLine($"key: {key}");
//    foreach (var pattern in patterns)
//    {
//        Console.WriteLine($"{pattern.p}:{pattern.o}:{pattern.v}:{pattern.next}");
//    }
//}

record struct Item (int x, int m, int a, int s);

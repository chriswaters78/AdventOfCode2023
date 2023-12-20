using System.Numerics;

var stateFlipFlop = new Dictionary<string, bool>();
var stateConjunction = new Dictionary<string, Dictionary<string,bool>>();

var modules = File.ReadAllLines("input.txt").Select(line => {
    var sp = line.Split(" -> ");

    Type type;
    string name;
    var outputs = sp[1].Split(", ").ToList();
    switch (sp[0])
    {
        case string flip when flip.StartsWith("%"):
            type = Type.Flipflop;
            name = sp[0][1..];
            stateFlipFlop.Add(name, false);
            break;
        case string conj when conj.StartsWith("&"):
            type = Type.Conjunction;
            name = sp[0][1..];
            stateConjunction[name] = new Dictionary<string, bool>();
            break;
        default:
            type = Type.Broadcaster;
            name = "broadcaster";
            break;
    };

    return new Module(type, name, outputs);
}).ToDictionary(md => md.name, md => md);

foreach (var conj in stateConjunction.Keys)
{
    //find all modules that connect to this
    foreach (var input in modules.Where(kvp => kvp.Value.outputs.Contains(conj)))
    {
        stateConjunction[conj].Add(input.Key, false);
    }
}

var signals = new Queue<Signal>();
long part1L = 0;
long part1H = 0;
long part1 = 0;
BigInteger part2 = 0;

var finalConjunctions = new[] { "db", "ln", "vq", "tf" };
var finalConjunctionsFiredAt = finalConjunctions.ToDictionary(str => str, _ => -1L);

for (long press = 1; press <= long.MaxValue; press++)
{
    if (press == 1001)
    {
        part1 = part1H * part1L;
    }
    if (finalConjunctionsFiredAt.All(kvp => kvp.Value != -1))
    {
        var numerator = finalConjunctionsFiredAt.Values.Aggregate(1L, (acc, i) => acc * i);
        var denominator = finalConjunctionsFiredAt.Values.Select(i => new BigInteger(i)).Aggregate(BigInteger.GreatestCommonDivisor);
        part2 = numerator / denominator;
        goto done;
    }

    part1L++;
    signals.Enqueue(new Signal("button", "broadcaster", false));
    while (signals.Count > 0)
    {
        var curr = signals.Dequeue();
        if (!modules.ContainsKey(curr.to))
        {
            continue;
        }
        var targetModule = modules[curr.to];
        switch (targetModule.type)
        {
            case Type.Broadcaster:
                foreach (var target in targetModule.outputs)
                {
                    if (curr.signal)
                        part1H++;
                    else
                        part1L++;
                    signals.Enqueue(new Signal(targetModule.name, target, curr.signal));
                }
                break;
            case Type.Conjunction:
                stateConjunction[targetModule.name][curr.from] = curr.signal;
                var output = !stateConjunction[targetModule.name].Values.All(b => b);
                foreach (var target in targetModule.outputs)
                {
                    if (!output && finalConjunctions.Contains(target) && finalConjunctionsFiredAt[target] == -1)
                    {
                        finalConjunctionsFiredAt[target] = press;
                    }
                    if (output)
                        part1H++;
                    else
                        part1L++;
                    signals.Enqueue(new Signal(targetModule.name, target, output));
                }
                break;
            case Type.Flipflop:
                if (curr.signal)
                    continue;
                stateFlipFlop[targetModule.name] = !stateFlipFlop[targetModule.name];
                foreach (var target in targetModule.outputs)
                {
                    if (stateFlipFlop[targetModule.name])
                        part1H++;
                    else
                        part1L++;
                    signals.Enqueue(new Signal(targetModule.name, target, stateFlipFlop[targetModule.name]));
                }
                break;
        }
    }
}
done:;
Console.WriteLine($"Part1: {part1}");
Console.WriteLine($"Part2: {part2}");

record struct Module(Type type, string name, List<string> outputs);
record struct Signal(string from, string to, bool signal);
enum Type
{
    Broadcaster,
    Conjunction,
    Flipflop
}

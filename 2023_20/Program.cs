using System.Linq;
using System.Text;

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
    foreach (var input in modules.Where(kvp => kvp.Value.targets.Contains(conj)))
    {
        stateConjunction[conj].Add(input.Key, false);
    }
}

var signals = new Queue<Signal>();

long part1L = 0;
long part1H = 0;
long part2 = 0;
int maxTp = 0;
for (long press = 1; press < long.MaxValue; press++)
{
    //Console.WriteLine($"Press {press}, lv = {stateFlipFlop["lv"]}, predicted {(press / 8) % 2 == 1}");

    //var tpCount = cjCount("vd");
    //if (tpCount > maxTp)
    //{
    //    maxTp = tpCount;
    //    Console.WriteLine($"vd count {cjCount("vd")} out of {stateConjunction["vd"].Values.Count()} at press {press}");
    //}
    //if (cjOn("vd"))
    //{
    //    Console.WriteLine($"vd on at {press}");
    //}
    //Console.WriteLine(printSingleCJ("tp"));
    //Console.WriteLine(printSingleCJ("bk"));
    //Console.WriteLine(printSingleCJ("pt"));
    //Console.WriteLine(printSingleCJ("vd"));
    //Console.WriteLine(printF(stateFlipFlop));
    //Console.WriteLine(printC(stateConjunction));
    part1L++;
    signals.Enqueue(new Signal("button", "broadcaster", false));
    while (signals.Count > 0)
    {
        var curr = signals.Dequeue();
        if (curr.to == "rx" )
        {
            if (!curr.signal)
            {
                part2 = press;
                goto done;
            }
        }
        if (!modules.ContainsKey(curr.to))
        {
            continue;
        }
        var targetModule = modules[curr.to];
        switch (targetModule.type)
        {
            case Type.Broadcaster:
                foreach (var target in targetModule.targets) 
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
                foreach (var target in targetModule.targets)
                {
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
                foreach (var target in targetModule.targets)
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
Console.WriteLine($"Part2: {part2}");
var part1 = part1H * part1L;

Console.WriteLine($"Part1: {part1}");

bool cjOn(string cj) => stateConjunction[cj].Values.All(b => b);
int cjCount(string cj) => stateConjunction[cj].Values.Count(b => b);

string printSingleCJ(string cj)
{
    StringBuilder sb = new StringBuilder();
    sb.AppendLine($"Conjunctions {cj}:");
    foreach (var kvp in stateConjunction[cj].OrderBy(kvp3 => kvp3.Key))
    {
        sb.Append($"{kvp.Key}={kvp.Value},");
    }
    return sb.ToString();
}

string printF(Dictionary<string, bool> flipFlops)
{
    StringBuilder sb = new StringBuilder();
    sb.Append("FlipFlops: ");
    foreach (var kvp in flipFlops.OrderBy(kvp => kvp.Key))
    {
        sb.Append($"{kvp.Key}={kvp.Value},");
    }
    return sb.ToString();
}
string printC(Dictionary<string, Dictionary<string,bool>> conjunctions)
{
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("Conjunctions:");
    foreach (var kvp in conjunctions.OrderBy(kvp => kvp.Key))
    {
        sb.Append($"Key: {kvp.Key}");
        foreach (var kvp2 in kvp.Value.OrderBy(kvp3 => kvp3.Key))
        {
            sb.Append($"{kvp2.Key}={kvp2.Value},");
        }
        sb.AppendLine();
    }
    return sb.ToString();
}

record struct Module(Type type, string name, List<string> targets);
record struct Signal(string from, string to, bool signal);
enum Type
{
    Broadcaster,
    Conjunction,
    Flipflop
}

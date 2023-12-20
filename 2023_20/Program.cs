using System.Numerics;

var modules = File.ReadAllLines("input.txt").Select(line => {
    var sp = line.Split(" -> ");
    var outputs = sp[1].Split(", ").ToList();
    switch (sp[0])
    {
        case string flip when flip.StartsWith("%"):
            return new Module(Type.Flipflop, sp[0][1..], outputs);
        case string conj when conj.StartsWith("&"):
            return new Module(Type.Conjunction, sp[0][1..], outputs);
        default:
            return new Module(Type.Broadcaster, "broadcaster", outputs);
    };
}).ToDictionary(md => md.name, md => md);

var states = modules.Keys.ToDictionary(name => name, _ => false);
var inputs = modules.Values.ToDictionary(m => m.name, m => modules.Values.Where(input => input.outputs.Any(output => m.name == output)).Select(input => input.name).ToList());

var finalConjunctionsFiredAt = new[] { "db", "ln", "vq", "tf" }.ToDictionary(str => str, _ => -1L);

(long part1L, long part1H, BigInteger part2) = (0, 0, 0);
for (long press = 1; press <= long.MaxValue; press++) 
{
    if (press == 1001) Console.WriteLine($"Part1: {part1H * part1L}");

    if (finalConjunctionsFiredAt.All(kvp => kvp.Value != -1))
    {
        //apparently they are always primes for the puzzle inputs
        part2 = finalConjunctionsFiredAt.Values.Aggregate((i1,i2) => i1 * i2)
            / finalConjunctionsFiredAt.Values.Select(i => (BigInteger) i).Aggregate(BigInteger.GreatestCommonDivisor);
        
        Console.WriteLine($"Part2: {part2}");
        return;
    }

    var queue = new Queue<Signal>( new[] { new Signal("broadcaster", false) });
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        
        if (current.signal) part1H++; else part1L++;
        if (!current.signal && finalConjunctionsFiredAt.TryGetValue(current.to, out long lastFired) && lastFired == -1)
            finalConjunctionsFiredAt[current.to] = press;

        if (!modules.ContainsKey(current.to))
            continue;

        var module = modules[current.to];      
        if (module.type == Type.Flipflop && current.signal)
            continue;

        var signalOutput = module.type switch
        {
            Type.Broadcaster => false,
            Type.Conjunction => !inputs[module.name].All(input => states[input]),
            Type.Flipflop => !states[module.name],
        };

        foreach (var output in module.outputs)
        {
            states[module.name] = signalOutput;
            queue.Enqueue(new Signal(output, signalOutput));
        }
    }
}

record struct Module(Type type, string name, List<string> outputs);
record struct Signal(string to, bool signal);
enum Type
{
    Broadcaster,
    Conjunction,
    Flipflop
}

﻿using System.Numerics;

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
modules.Add("rx", new Module(Type.Broadcaster, "rx", new List<string>()));

var states = modules.Keys.ToDictionary(name => name, _ => false);
var inputs = modules.Values.ToDictionary(m => m.name, m => modules.Values.Where(input => input.outputs.Any(output => m.name == output)).Select(input => input.name).ToList());

var finalConjunctions = new[] { "db", "ln", "vq", "tf" }.ToDictionary(str => str, _ => -1L);

(long part1L, long part1H) = (0, 0);
for (long press = 1; press <= long.MaxValue; press++) 
{
    var queue = new Queue<Signal>( new[] { new Signal("broadcaster", false) });
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        
        //record what we need to know to answer the question
        if (current.signal) part1H++; else part1L++;
        if (!current.signal && finalConjunctions.TryGetValue(current.to, out long lastFired) && lastFired == -1)
            finalConjunctions[current.to] = press;

        var module = modules[current.to];      
        if (module.type == Type.Flipflop && current.signal)
            continue;

        var signalOutput = module.type switch
        {
            Type.Broadcaster => current.signal,
            Type.Conjunction => !inputs[module.name].All(input => states[input]),
            Type.Flipflop => !states[module.name],
        };

        foreach (var output in module.outputs)
        {
            states[module.name] = signalOutput;
            queue.Enqueue(new Signal(output, signalOutput));
        }
    }

    //check if we are done on either part
    if (press == 1000) Console.WriteLine($"Part1: {part1H * part1L}");
    if (finalConjunctions.All(kvp => kvp.Value != -1))
    {
        //we have found the cycle length for our four outputs
        //apparently they are always primes for the puzzle inputs but find the LCM in case they are not
        var part2 = finalConjunctions.Values.Aggregate((i1, i2) => i1 * i2)
            / finalConjunctions.Values.Select(i => (BigInteger)i).Aggregate(BigInteger.GreatestCommonDivisor);

        Console.WriteLine($"Part2: {part2}");
        return;
    }
}

record struct Module(Type type, string name, List<string> outputs);
record struct Signal(string to, bool signal);
enum Type { Broadcaster, Conjunction, Flipflop }

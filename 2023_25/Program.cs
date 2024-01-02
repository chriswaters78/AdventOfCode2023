using System.Diagnostics;
using System.Linq;
double Sqrt2 = Math.Sqrt(2);
Random rand = new Random();
var stopwatch = new Stopwatch();

Dictionary<string, List<string>> originalGraphStr = null;
Dictionary<int, List<int>> originalGraph = null;
Dictionary<int, string> intToKeyMap;

var runAgainstRandomGraph = bool.Parse(args[0]);
if (runAgainstRandomGraph)
{
    originalGraph = _2023_25.Graph.GenerateRandomHyperbolicGraph(int.Parse(args[1]), 3, 500);
    //originalGraph = _2023_25.Graph.SimpleGraph();
    intToKeyMap = originalGraph.Keys.ToDictionary(i => i, i => i.ToString());
    originalGraphStr = originalGraph.ToDictionary(kvp => intToKeyMap[kvp.Key], kvp => kvp.Value.Select(i => intToKeyMap[i]).ToList());

}
else
{
    originalGraphStr = new Dictionary<string, List<string>>();
    foreach (var line in File.ReadAllLines("input.txt"))
    {
        //jqt: rhn xhk nvd
        var sp = line.Split(": ");
        if (!originalGraphStr.ContainsKey(sp[0]))
            originalGraphStr[sp[0]] = new List<string>();
        foreach (var edge in sp[1].Split(" "))
        {
            originalGraphStr[sp[0]].Add(edge);
            if (!originalGraphStr.ContainsKey(edge))
                originalGraphStr[edge] = new List<string>();
            originalGraphStr[edge].Add(sp[0]);
        }
    }
    intToKeyMap = originalGraphStr.Keys.Select((str, i) => (str, i)).ToDictionary(tp => tp.i, tp => tp.str);
    var keyToIntMap = originalGraphStr.Keys.Select((str, i) => (str, i)).ToDictionary(tp => tp.str, tp => tp.i);
    originalGraph = originalGraphStr.ToDictionary(kvp => keyToIntMap[kvp.Key], kvp => kvp.Value.Select(edge => keyToIntMap[edge]).ToList());
}

Console.WriteLine($"{originalGraph.Keys.Count} nodes, edges {originalGraph.Values.Sum(list => list.Count)}");

var tests = new (string name, Func<(int minCut, List<int> partition)>)[] {
    
        //("Find 4 Connected Set", () => _2023_25.Find4ConnectedSet.MinimumCut(originalGraph)),
        //("Count crossings", () => _2023_25.CountCrossings.MinimumCut(originalGraph, 50, 3)),
        //("Distinct routes", () => _2023_25.DistinctRoutes.MinimumCut(originalGraph)),
        //("Stoer-Wagner", () => _2023_25.StoerWagner.MinimumCut(originalGraph.AsReadOnly())),
        //OST version seems slower for some reason?
        ("Karger-Stein OST", () => _2023_25.KargerStein_OST.MinimumCut(originalGraph.AsReadOnly(), !runAgainstRandomGraph ? 3 : int.MaxValue - 1, true, 2.1, 6)),
        ("Karger-Stein", () => _2023_25.KargerStein.MinimumCut(originalGraph.AsReadOnly(), !runAgainstRandomGraph ? 3 : int.MaxValue - 1, true, 2.1, 6)),
        //("Karger", () => _2023_25.KargerStein.MinimumCut(originalGraph.AsReadOnly(), 3, false, -1,-1))
    };

var averageRuntimes = new long[tests.Length];
HashSet<string> prevSet1 = null;
HashSet<string> prevSet2 = null;
for (int i = 0; i < int.MaxValue; i++)
{
    Console.WriteLine($"########### Iteration {i} ###########");
    foreach (var ((name, resultFactory), index) in tests.Select((test,i) => (test,i)))
    {
        stopwatch.Restart();
        (var minCut, List<int> partition) = resultFactory();
        stopwatch.Stop();
        averageRuntimes[index] = (averageRuntimes[index] * i + stopwatch.ElapsedMilliseconds) / (i + 1);
        
        var set1 = partition.Select(i => intToKeyMap[i]).ToHashSet();
        var set2 = originalGraphStr.Keys.Where(key => !set1.Contains(key)).ToHashSet();

        if (minCut == 3 && (set1.Count != 6 && set2.Count != 6))
        {
        }

        Console.WriteLine($"{name}: min cut {minCut}, partition {set1.Count} <=> {set2.Count}, average: {averageRuntimes[index]}ms, this: {stopwatch.ElapsedMilliseconds}ms");

        (set1, set2) = set1.Count <= set2.Count ? (set1, set2) : (set2, set1);
        if (prevSet1 != null)
        {
            if (!prevSet1.OrderBy(str => str).SequenceEqual(set1.OrderBy(str => str)))
            {
                Console.WriteLine("*** set did not match previous found set ***");
            }
        }
        else
        {
            //Console.WriteLine($"Found two sets with mincut of {minCut}");
            //Console.WriteLine($"Set 1: [{String.Join(",", set1)}]");
            //Console.WriteLine($"Set 2: [{String.Join(",", set2)}]");
            
            //var edges = originalGraphStr.SelectMany(kvp => kvp.Value.Select(to => (kvp.Key, to)))
            //    .Where(tp => set1.Contains(tp.Key) && set2.Contains(tp.to) || set2.Contains(tp.Key) && set1.Contains(tp.to))
            //    .Select(tp => tp.Key.CompareTo(tp.to) == -1 ? (tp.Key, tp.to) : (tp.to, tp.Key))
            //    .Distinct().ToList<(string from, string to)>();
            
            //Console.WriteLine($"Min cut edges = {String.Join("; ", edges.Select(tp => $"{tp.from} => {tp.to}"))}");
        }
        (prevSet1, prevSet2) = (set1, set2);
    }
}



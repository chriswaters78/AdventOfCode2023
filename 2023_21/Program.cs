using System.Collections.Concurrent;
using System.Numerics;

var grid = File.ReadAllLines("input.txt").SelectMany((str, r) => str.Select((ch, c) => (ch, new Complex(r, c)))).ToDictionary(tp => tp.Item2, tp => tp.ch);

(int R, int C) = (grid.Keys.Max(p => (int) p.Real), grid.Keys.Max(p => (int)p.Imaginary));
var start = grid.Where(kvp => kvp.Value == 'S').Single().Key;
grid[start] = '.';

var offsets = new Complex[] { new Complex(1, 0), new Complex(0, 1), new Complex(-1, 0), new Complex(0, -1) };

var canReachIn = new ConcurrentDictionary<int, Dictionary<Complex, HashSet<Complex>>>();
canReachIn[0] = grid.Keys.ToDictionary(key => key, key => new HashSet<Complex>(new[] { key }));

canReachIn[1] = grid.Keys.Select(sp => (sp, offsets.Select(os => sp + os).Where( point => inBounds(point) && grid[point] != '#')))
    .GroupBy(tp => tp.sp)
    .ToDictionary(grp => grp.Key, grp => new HashSet<Complex>(grp.SelectMany(tp => tp.Item2)));

int toReach = 64;
var maxI = Math.Log2(64);
for (int i = 1; i <= maxI; i++)
{
    var step = (int)Math.Pow(2, i);
    canReachIn[step] = grid.Keys.ToDictionary(key => key, _ => new HashSet<Complex>());

    Parallel.For(0, R, r =>
    {
        Console.WriteLine($"Step {step} row {r} complete");
        for (int c = 0; c < C; c++)
        {
            var point = new Complex(r, c);
            if (grid[point] == '#')
                continue;

            //say we are checking i = 2
            //which is those that can be reached in 4
            //this equals those that can be reached from those that can be reached in 2
            var prevStep = (int)Math.Pow(2, i - 1);
            foreach (var p1 in canReachIn[prevStep][point])
            {
                foreach (var p2 in canReachIn[prevStep][p1])
                {
                    canReachIn[step][point].Add(p2);
                }
            }
            //canReachIn[step][point] = canReachIn[prevStep][point].SelectMany(pp => canReachIn[prevStep][pp]).Distinct().ToList();

            //Console.WriteLine($"Processed {r},{c} for step {step}. Found {canReachIn[step][point].Count}");
        }
    });
}

var canReach = new HashSet<Complex>();
for (int i = 6; i >= 0; i--)
{
    var step = (int) Math.Pow(2, i);
    if (toReach / step > 0)
    {
        if (canReach.Count == 0)
        {
            canReach = new HashSet<Complex>(canReachIn[step][start]);
        }
        else
        {
            var newCanReach = new HashSet<Complex>();
            foreach (var p1 in canReach)
            {
                foreach (var p2 in canReachIn[step][p1])
                {
                    newCanReach.Add(p2);
                }
            }
            canReach = newCanReach;
        }
        toReach -= step;
    }
    Console.WriteLine($"Tostep {step} done");
}


//we have a grid of everything that can be reached in integer steps

Console.WriteLine($"Part1: {canReach.Count}");

bool inBounds(Complex p) => p.Real >= 0 && p.Real < R && p.Imaginary >= 0 && p.Imaginary < C;
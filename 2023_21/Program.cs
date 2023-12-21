using System.Collections.Concurrent;
using System.Numerics;

var grid = File.ReadAllLines("input.txt").SelectMany((str, r) => str.Select((ch, c) => (ch, new Point(r, c)))).ToDictionary(tp => tp.Item2, tp => tp.ch);

(int R, int C) = (grid.Keys.Max(p => p.r) + 1, grid.Keys.Max(p => p.c) + 1);
var start = grid.Where(kvp => kvp.Value == 'S').Single().Key;
grid[start] = '.';

var offsets = new Point[] { new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1) };

int toReach = 64;
var maxI = (int) Math.Log2(toReach) + 1;
var canReachIn = new HashSet<Point>[maxI + 1][,];
canReachIn[0] = new HashSet<Point>[R, C];
canReachIn[1] = new HashSet<Point>[R, C];

foreach (var key in grid.Keys)
{
    canReachIn[0][key.r, key.c] = new HashSet<Point>(new[] { key });
    canReachIn[1][key.r, key.c] = new HashSet<Point>();
    foreach (var key2 in offsets.Select(os => new Point(key.r + os.r, key.c + os.c)).Where(point => inBounds(point) && grid[point] != '#'))
    {
        canReachIn[1][key.r, key.c].Add(key2);
    }
}
//canReachIn[0] = grid.Keys.ToDictionary(key => key, key => new HashSet<Point>(new[] { key }));

//canReachIn[1] = grid.Keys.Select(sp => (sp, offsets.Select(os => new Point(sp.r + os.r, sp.c + os.c))
//    .Where(point => inBounds(point) && grid[point] != '#')))
//    .GroupBy(tp => tp.sp)
//    .ToDictionary(grp => grp.Key, grp => new HashSet<Point>(grp.SelectMany(tp => tp.Item2)));

for (int i = 2; i <= maxI; i++)
{
    var step = (int)Math.Pow(2, i);
    canReachIn[i] = new HashSet<Point>[R, C]; //grid.Keys.ToDictionary(key => key, _ => new HashSet<Point>());

    Parallel.For(0, R, r =>
    {
        Console.WriteLine($"Step {step} row {r} complete");
        for (int c = 0; c < C; c++)
        {
            var point = new Point(r, c);
            if (grid[point] == '#')
                continue;

            canReachIn[i][r, c] = new HashSet<Point>();

            //say we are checking i = 2
            //which is those that can be reached in 4
            //this equals those that can be reached in 2 from those that can be reached in 2
            //var prevStep = (int)Math.Pow(2, i - 1);
            foreach (var p1 in canReachIn[i-1][point.r,point.c])
            {
                //can reach r,c from p1 in i-1
                foreach (var p2 in canReachIn[i - 1][p1.r, p1.c])
                {
                    //can reach p1 from p2 in i-1
                    canReachIn[i][point.r,point.c].Add(p2);
                }
            }
            //Console.WriteLine($"Processed {r},{c} for step {step}. Found {canReachIn[step][point].Count}");
        }
    });
}

//canReachIn[0] is those that can be reached in  0 moves
//canReachIn[1] is those that can be reached in 2^0 = 1 moves
//canReachIn[2] is those that can be reached in 2^2 = 2 moves

var canReach = new HashSet<Point>();
for (int i = maxI; i >= 1; i--)
{
    var step = (int) Math.Pow(2, i);
    if (toReach / step > 0)
    {
        if (canReach.Count == 0)
        {
            canReach = new HashSet<Point>(canReachIn[i + 1][start.r, start.c]);
        }
        else
        {
            var newCanReach = new HashSet<Point>();
            foreach (var p1 in canReach)
            {
                foreach (var p2 in canReachIn[i + 1][p1.r, p1.c])
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

bool inBounds(Point p) => p.r >= 0 && p.r < R && p.c >= 0 && p.c < C;

record struct Point(int r, int c);
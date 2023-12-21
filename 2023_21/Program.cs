using System.Collections.Concurrent;
using System.Numerics;
using System.Text;

var grid = File.ReadAllLines("input.txt").SelectMany((str, r) => str.Select((ch, c) => (ch, new Point(r, c)))).ToDictionary(tp => tp.Item2, tp => tp.ch);
int maxToReach = 130;
var maxI = (int)Math.Log2(maxToReach);

(int R, int C) = (grid.Keys.Max(p => p.r) + 1, grid.Keys.Max(p => p.c) + 1);
var start = grid.Where(kvp => kvp.Value == 'S').Single().Key;
grid[start] = '.';

var offsets = new Point[] { new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1) };

//full grid = 7521



return;

//canReach[i] = points we can reach in 2^i steps
var canReachIn = new HashSet<Point>[maxI + 1][,];
canReachIn[0] = new HashSet<Point>[R, C];

foreach (var key in grid.Keys)
{    
    canReachIn[0][key.r, key.c] = new HashSet<Point>();
    foreach (var key2 in offsets.Select(os => new Point(key.r + os.r, key.c + os.c)).Where(point => inBounds(point) && grid[point] != '#'))
    {
        canReachIn[0][key.r, key.c].Add(key2);
    }
}

for (int i = 1; i <= maxI; i++)
{
    var step = (int)Math.Pow(2, i);
    canReachIn[i] = new HashSet<Point>[R, C];

    Parallel.For(0, R, r =>
    {
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

    Console.WriteLine($"Step {Math.Pow(2,i)} complete");
    Console.WriteLine(print(i));
}

for (var toReach = 1; toReach <= maxToReach; toReach++)
{
    var canReach = new HashSet<Point>();
    var dec = toReach;
    for (int i = maxI; i >= 0; i--)
    {
        var step = (int)Math.Pow(2, i);
        if (dec / step > 0)
        {
            if (canReach.Count == 0)
            {
                canReach = new HashSet<Point>(canReachIn[i][start.r, start.c]);
            }
            else
            {
                var newCanReach = new HashSet<Point>();
                foreach (var p1 in canReach)
                {
                    foreach (var p2 in canReachIn[i][p1.r, p1.c])
                    {
                        newCanReach.Add(p2);
                    }
                }
                canReach = newCanReach;
            }
            dec -= step;
        }
    }

    Console.WriteLine($"Found final grid for {toReach} steps, {canReach.Count} total");
    Console.WriteLine(printA(canReach));
}

string printA(HashSet<Point> points)
{
    StringBuilder sb = new StringBuilder();
    for (int r = 0; r < R; r++)
    {
        for (int c = 0; c < C; c++)
        {
            var point = new Point(r, c);

            sb.Append(grid[point] switch
            {
                var ch when point == start => 'S',
                '#' => '#',
                var _ => points.Contains(point) ? 'O' : '.'
            });
        }
        sb.AppendLine();
    }

    return sb.ToString();
}
string print(int step)
{
    StringBuilder sb = new StringBuilder();
    for (int r = 0; r < R; r++)
    {
        for (int c = 0; c < C; c++)
        {
            var point = new Point(r, c);
            
            sb.Append(grid[point] switch
            {
                var ch when point == start => 'S',
                '#' => '#',
                var _ => canReachIn[step][r, c].Contains(start) ? 'O' : '.'
            });
        }
        sb.AppendLine();
    }

    return sb.ToString();
}

bool inBounds(Point p) => p.r >= 0 && p.r < R && p.c >= 0 && p.c < C;

record struct Point(int r, int c);
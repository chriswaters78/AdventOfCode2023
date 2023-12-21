using System.Diagnostics;
using System.Text;
Stopwatch watch = new Stopwatch();
watch.Start();

var grid = File.ReadAllLines("input.txt").SelectMany((str, r) => str.Select((ch, c) => (ch, new Point(r, c)))).ToDictionary(tp => tp.Item2, tp => tp.ch);
int maxToReach = 131;
var maxI = (int)Math.Log2(maxToReach);

(int R, int C) = (grid.Keys.Max(p => p.r) + 1, grid.Keys.Max(p => p.c) + 1);
var start = grid.Where(kvp => kvp.Value == 'S').Single().Key;
Console.WriteLine($"Start point: ({start.r},{start.c}), R: {R} C: {C}");
grid[start] = '.';

var offsets = new Point[] { new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1) };

//canReach[i] is the points we can reach in 2^i steps
var canReachIn = new HashSet<Point>[maxI + 1][,];
canReachIn[0] = new HashSet<Point>[R, C];

foreach (var key in grid.Keys)
{    
    canReachIn[0][key.r, key.c] = [.. offsets.Select(os => new Point(key.r + os.r, key.c + os.c)).Where(point => inBounds(point) && grid[point] != '#')];
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
        }
    });

    Console.WriteLine($"Step {Math.Pow(2,i)} complete");
}



var findThese = new[] { 65, 131};
var canReach = findThese.ToDictionary(i => i, _ => new HashSet<Point>());
foreach (var findThis in findThese)
{
    var dec = findThis;
    for (int i = maxI; i >= 0; i--)
    {
        var step = (int)Math.Pow(2, i);
        if (dec / step > 0)
        {
            if (canReach[findThis].Count == 0)
            {
                canReach[findThis] = new HashSet<Point>(canReachIn[i][start.r, start.c]);
            }
            else
            {
                var newCanReach = new HashSet<Point>();
                foreach (var p1 in canReach[findThis])
                {
                    foreach (var p2 in canReachIn[i][p1.r, p1.c])
                    {
                        newCanReach.Add(p2);
                    }
                }
                canReach[findThis] = newCanReach;
            }
            dec -= step;
        }
    }
}
foreach (var kvp in canReach)
{
    Console.WriteLine($"Grid after {kvp.Key} steps, count {kvp.Value.Count}");
    Console.WriteLine(printA(kvp.Value));

}

//argh there is one point 117,12 that is fully surrounded!

List<Point> cornerPoints = new List<Point>();
//we need to find the count of each corner we can chop off
int topLeft = 0;
for (int r = 0; r < 65; r++)
{
    for (int c = 0; c < 65 - r; c++)
    {
        //it would be filled if manhattan distance is odd, and there is a place on the grid
        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
        {
            cornerPoints.Add(new Point(r, c));
            topLeft++;
        }
    }
}
int topRight = 0;
for (int r = 0; r < 65; r++)
{
    for (int c = 66 + r; c < 131; c++)
    {
        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
        {
            cornerPoints.Add(new Point(r, c));
            topRight++;
        }
    }
}
int bottomLeft = 0;
for (int r = 66; r < 131; r++)
{
    for (int c = 0; c < r - 65; c++)
    {
        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
        {
            cornerPoints.Add(new Point(r, c));
            bottomLeft++;
        }
    }
}
int bottomRight = 0;
for (int r = 66; r < 131; r++)
{
    for (int c = 196 - r; c < 131; c++)
    {
        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
        {
            cornerPoints.Add(new Point(r, c));
            bottomRight++;
        }
    }
}

Console.WriteLine($"Corner points total {cornerPoints.Count}, distinct {cornerPoints.Distinct()}, corner totals {topLeft + topRight + bottomLeft + bottomRight}");
Console.WriteLine($"Corner quantities: TL:{topLeft}, TR: {topRight}, BL: {bottomLeft}, BR: {bottomRight}");

var diff = cornerPoints.Concat(canReach[65]).Except(canReach[131]).ToList();
Console.WriteLine($"Found {diff.Count} in [corners + 65] that are not in [131]");

Console.WriteLine(String.Join(", ", diff.Select(p => $"({p.r},{p.c})")));

var diff2 = canReach[131].Except(cornerPoints).Except(canReach[65]).ToList();
Console.WriteLine($"Found {diff2.Count} in [131 - corners - 65]");
Console.WriteLine(String.Join(", ", diff2.Select(p => $"({p.r},{p.c})")));


//ok we have our corners
//first 65 steps makes a diamond that fills the first grid
//then every 131 steps after that, we shift the points of the diamond one full grid away
//26501365 is the number of steps
//which exactly equals 65 + 131 * 202300

//of form 

//we have (202300 * 2 + 1) rows in total
var totalRows = 202300L * 2 + 1;
Console.WriteLine($"TotalRows: {totalRows}");
//total full rows excluding middle row
var sum = 2 * Enumerable.Range(1, 202300).Sum(i => 2 * (i - 1) + 1L) * canReach[131].Count;
Console.WriteLine($"Sum1: {sum}");
//and we take off 202300 * (topLeft + topRight + bottomLeft + bottomRight)
sum -= 202300 * (topLeft + topRight + bottomLeft + bottomRight);
Console.WriteLine($"Sum2: {sum}");
//add on the middle row
sum += totalRows;
Console.WriteLine($"Sum3: {sum}");
//and take off the four corners that are all missing on the middle row
sum -= (topLeft + topRight + bottomLeft + bottomRight);

Console.WriteLine($"Final Answer: {sum}");

//so line is above middle is /..202300..\, i.e. 202300 full grids 
//so line below middle is \..202300../, i.e. 202300 full grids 
// for top half we have 2 + 4 + 6.... 202302 full grids - 202302 * (tl + tr)
// for top half we have 2 + 4 + 6.... 202302 full grids - 202302 * (bl + br)
//long answer = 2 * sum * canReach[131].Count - 202302 * (topLeft + topRight + bottomLeft + bottomRight);

//  611177534976212 TOO LOW!!!
////153251870640390 TOO LOW!
//Console.WriteLine($"Part2: {answer}");


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
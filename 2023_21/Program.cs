using System.Diagnostics;
using System.Text;
Stopwatch watch = new Stopwatch();
watch.Start();

var grid = File.ReadAllLines("input.txt").SelectMany((str, r) => str.Select((ch, c) => (ch, new Point(r, c)))).ToDictionary(tp => tp.Item2, tp => tp.ch);
int maxToReach = 131;
//var maxI = (int)Math.Log2(maxToReach);

var offsets = new Point[] { new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1) };

(int R, int C) = (grid.Keys.Max(p => p.r) + 1, grid.Keys.Max(p => p.c) + 1);
var start = grid.Where(kvp => kvp.Value == 'S').Single().Key;
Console.WriteLine($"Start point: ({start.r},{start.c}), R: {R} C: {C}");
grid[start] = 'O';

var canReachIn = new Dictionary<int, HashSet<Point>>();
canReachIn[0] = new HashSet<Point>(new[] { start });
for (int steps = 1; steps <= maxToReach; steps++)
{
    canReachIn[steps] = new HashSet<Point>();
    foreach (var p in canReachIn[steps - 1])
    {
        foreach (var offset in offsets)
        {
            var point = new Point(p.r + offset.r, p.c + offset.c);

            if (inBounds(point) && grid[point] != '#')
            {
                canReachIn[steps].Add(point);
            }
        }
    }
}

foreach (var kvp in canReachIn)
{
    Console.WriteLine($"Step {kvp.Key}, reached {kvp.Value.Count} points");
}

//there is one point 117,12 that is fully surrounded!!!

//List<Point> cornerPoints = new List<Point>();
////we need to find the count of each corner we can chop off
//int topLeft = 0;
//for (int r = 0; r < 65; r++)
//{
//    for (int c = 0; c < 65 - r; c++)
//    {
//        //it would be filled if manhattan distance is odd, and there is a place on the grid
//        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
//        {
//            cornerPoints.Add(new Point(r, c));
//            topLeft++;
//        }
//    }
//}
//int topRight = 0;
//for (int r = 0; r < 65; r++)
//{
//    for (int c = 66 + r; c < 131; c++)
//    {
//        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
//        {
//            cornerPoints.Add(new Point(r, c));
//            topRight++;
//        }
//    }
//}
//int bottomLeft = 0;
//for (int r = 66; r < 131; r++)
//{
//    for (int c = 0; c < r - 65; c++)
//    {
//        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
//        {
//            cornerPoints.Add(new Point(r, c));
//            bottomLeft++;
//        }
//    }
//}
//int bottomRight = 0;
//for (int r = 66; r < 131; r++)
//{
//    for (int c = 196 - r; c < 131; c++)
//    {
//        if ((r + c) % 2 == 1 && canReach[131].Contains(new Point(r, c)))
//        {
//            cornerPoints.Add(new Point(r, c));
//            bottomRight++;
//        }
//    }
//}

//Console.WriteLine($"Corner points total {cornerPoints.Count}, distinct {cornerPoints.Distinct()}, corner totals {topLeft + topRight + bottomLeft + bottomRight}");
//Console.WriteLine($"Corner quantities: TL:{topLeft}, TR: {topRight}, BL: {bottomLeft}, BR: {bottomRight}");

//var diff = cornerPoints.Concat(canReach[65]).Except(canReach[131]).ToList();
//Console.WriteLine($"Found {diff.Count} in [corners + 65] that are not in [131]");

//Console.WriteLine(String.Join(", ", diff.Select(p => $"({p.r},{p.c})")));

//var diff2 = canReach[131].Except(cornerPoints).Except(canReach[65]).ToList();
//Console.WriteLine($"Found {diff2.Count} in [131 - corners - 65]");
//Console.WriteLine(String.Join(", ", diff2.Select(p => $"({p.r},{p.c})")));


////ok we have our corners
////first 65 steps makes a diamond that fills the first grid
////then every 131 steps after that, we shift the points of the diamond one full grid away
////26501365 is the number of steps
////which exactly equals 65 + 131 * 202300

////of form 

////we have (202300 * 2 + 1) rows in total
//var totalRows = 202300L * 2 + 1;
//Console.WriteLine($"TotalRows: {totalRows}");
////total full rows excluding middle row
//var sum = 2 * Enumerable.Range(1, 202300).Sum(i => 2 * (i - 1) + 1L) * canReach[131].Count;
//Console.WriteLine($"Sum1: {sum}");
////and we take off 202300 * (topLeft + topRight + bottomLeft + bottomRight)
//sum -= 202300 * (topLeft + topRight + bottomLeft + bottomRight);
//Console.WriteLine($"Sum2: {sum}");
////add on the middle row
//sum += totalRows;
//Console.WriteLine($"Sum3: {sum}");
////and take off the four corners that are all missing on the middle row
//sum -= (topLeft + topRight + bottomLeft + bottomRight);

//Console.WriteLine($"Final Answer: {sum}");

////so line is above middle is /..202300..\, i.e. 202300 full grids 
////so line below middle is \..202300../, i.e. 202300 full grids 
//// for top half we have 2 + 4 + 6.... 202302 full grids - 202302 * (tl + tr)
//// for top half we have 2 + 4 + 6.... 202302 full grids - 202302 * (bl + br)
////long answer = 2 * sum * canReach[131].Count - 202302 * (topLeft + topRight + bottomLeft + bottomRight);

////  611177534976212 TOO LOW!!!
//////153251870640390 TOO LOW!
////Console.WriteLine($"Part2: {answer}");


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
//string print(int step)
//{
//    StringBuilder sb = new StringBuilder();
//    for (int r = 0; r < R; r++)
//    {
//        for (int c = 0; c < C; c++)
//        {
//            var point = new Point(r, c);
            
//            sb.Append(grid[point] switch
//            {
//                var ch when point == start => 'S',
//                '#' => '#',
//                var _ => canReachIn[step][r, c].Contains(start) ? 'O' : '.'
//            });
//        }
//        sb.AppendLine();
//    }

//    return sb.ToString();
//}

bool inBounds(Point p) => p.r >= 0 && p.r < R && p.c >= 0 && p.c < C;

record struct Point(int r, int c);
using System.Text;

var grid = File.ReadAllLines("input.txt").SelectMany((str, r) => str.Select((ch, c) => (ch, new Point(r, c)))).ToDictionary(tp => tp.Item2, tp => tp.ch);
var gridSize = (int)Math.Sqrt(grid.Count);
(int min, int max) = (0,gridSize);
var start = grid.Where(kvp => kvp.Value == 'S').Single().Key;
Console.WriteLine($"Start point: ({start.r},{start.c}), min/max: {min}:{max}");
grid[start] = '.';

int gridScale = 5;
int maxToReach = (gridSize / 2) + (gridScale - 1)*gridSize;
var offsets = new Point[] { new Point(1, 0), new Point(0, 1), new Point(-1, 0), new Point(0, -1) };

int noGrids = 2 * (gridScale - 1) + 1;
var newGrid = new Dictionary<Point, char>();
foreach (var key in grid.Keys)
{
    for (int rs = -(gridScale - 1); rs <= (gridScale - 1); rs++)
    {
        for (int cs = -(gridScale - 1); cs <= (gridScale - 1); cs++)
        {
            newGrid[new Point(key.r + rs * max, key.c + cs * max)] = grid[key];
        }
    }
}
grid = newGrid;
grid[start] = 'O';

(min, max) = (grid.Keys.Min(p => p.r), grid.Keys.Max(p => p.r) + 1);


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

            if (/*inBounds(point) && */ grid[point] != '#')
            //if (grid.TryGetValue(point, out char ch) && ch != '#')
            {
                canReachIn[steps].Add(point);
            }
        }
    }
}

//Wolfram alpha - quadratic fit calculator for:
//{1, 2, 3} xvalues
//{33833,93864,183871} y values
//14988 x^2 + 15067x + 3778 
long part22 = 202300L * 202300 * 14988 + 202300L * 15067 + 3778;
Console.WriteLine($"Part2 directly calculated: {part22}");


//first 65 steps makes a diamond that fills the first grid
//then every 131 steps after that, we shift the points of the diamond one full grid away but the pattern alternates between R1 and R2

//26501365 is the number of steps
//which exactly equals 65 + 131 * 202300

//26501365
var parameters = Enumerable.Range(1, 202300).Select(i => (long) i)
    .Select(i => new { i = i, R1 = (i + 1) * (i + 1), R2 = i * i })
    .Select(a => new { i = a.i, R1 = a.R1, R2 = a.R2, O = ((2 * a.i + 1) * (2 * a.i + 1) - a.R1 - a.R2) / 2 })
    .Select(a => (a, a.R1 * 3778 + a.R2 * 3699 + a.O * 7511))
    .ToList();

foreach (var para in parameters.Take(50).Concat(parameters.Skip(parameters.Count - 1)))
{
    Console.WriteLine($"{gridSize / 2} + {para.a.i} * 131 = {(gridSize / 2) + para.a.i * gridSize}, {para.Item2}");
}

var part2 = parameters.Last().Item2;

string printSet(HashSet<Point> points)
{
    StringBuilder sb = new StringBuilder();
    for (int r = min; r < max; r++)
    {
        for (int c = min; c < max; c++)
        {
            var point = new Point(r, c);

            sb.Append(grid[point] switch
            {
                var ch when point == start => 'S',
                var ch when points.Contains(point) => 'O',
                var ch => grid[point],
            });
        }
        sb.AppendLine();
    }

    return sb.ToString();
}

bool inBounds(Point p) => p.r >= min && p.r < max && p.c >= min && p.c < max;

record struct Point(int r, int c);
var neighbours = new Dictionary<char, Func<Point, Point[]>>() {
    { '|', p => new Point[] { new (p.x + 0, p.y - 1), new (p.x + 0, p.y + 1) } },
    { '-', p => new Point[] { new (p.x - 1, p.y + 0), new (p.x + 1, p.y + 0) } },
    { 'L', p => new Point[] { new (p.x + 0, p.y - 1), new (p.x + 1, p.y + 0) } },
    { 'J', p => new Point[] { new (p.x + 0, p.y - 1), new (p.x - 1, p.y + 0) } },
    { '7', p => new Point[] { new (p.x - 1, p.y + 0), new (p.x + 0, p.y + 1) } },
    { 'F', p => new Point[] { new (p.x + 0, p.y + 1), new (p.x + 1, p.y + 0) } },
};

var watch = new System.Diagnostics.Stopwatch();
watch.Start();
var file = File.ReadAllLines("input.txt");
(int H, int W) = (file.Length , file.First().Length );

char[][] grid = new char[H][];
//grid[0] = Enumerable.Repeat('.', W).ToArray();
foreach (var (line, i) in file.Select((line, i) => (line, i)))
{
    grid[i] = line.ToArray();
}
//grid[grid.Length - 1] = Enumerable.Repeat('.', W).ToArray();

var start = Enumerable.Range(0, H).SelectMany(y => Enumerable.Range(0, W).Select(x => new Point(x, y))).Single(p => grid[p.y][p.x] == 'S');
grid[start.y][start.x] = neighbours.Keys.First(c => neighbours[c](start).All(e => neighbours[grid[e.y][e.x]](e).Any(p => p == start)));

var queue = new Queue<Point>(new[] { start });
var visited = new HashSet<Point>(new[] { start });
while (true)
{
    var end = queue.Dequeue();
    //we expand both ends of the loop at the same time, and stop when these reach each other
    visited.Add(end);
    var newEnds = neighbours[grid[end.y][end.x]](end);
    if (newEnds.All(end => visited.Contains(end)))
    {
        break;
    }
    foreach (var next in newEnds.Where(p => !visited.Contains(p)))
    {
        queue.Enqueue(next);
    }
}

Console.WriteLine($"Part1: {Math.Ceiling(visited.Count  / 2m)} in {watch.ElapsedMilliseconds}ms");

//set all non-path characters to .
grid = Enumerable.Range(0,H).Select(y => Enumerable.Range(0,W).Select(x => visited.Contains(new Point(x,y)) ? grid[y][x] : '.').ToArray()).ToArray();

//single pass row scan from R to L counting any empty spaces where crossing count is odd
var part2 = Enumerable.Range(0, H).Sum(y =>
{
    int count = 0;
    int crossings = 0;
    char entered = '.';
    for (int x = W - 1; x >= 0; x--)
    {
        switch ((grid[y][x], entered))
        {
            case ('7', _):
            case ('J', _):
                entered = grid[y][x];
                break;
            case ('L', '7'):
            case ('F', 'J'):
                crossings++;
                entered = '.';
                break;
            case ('F', '7'):
            case ('L', 'J'):
                entered = '.';
                break;
            case ('|', _):
                crossings++;
                break;
            default:
                break;
        }
        count += (crossings % 2 == 1 && grid[y][x] == '.') ? 1 : 0;
    }
    return count;
});

Console.WriteLine($"Part2: {part2} in {watch.ElapsedMilliseconds}ms");
record struct Point(int x, int y);

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

char[][] grid = file.Select(line => line.ToArray()).ToArray();

var start = Enumerable.Range(0, H).SelectMany(y => Enumerable.Range(0, W).Select(x => new Point(x, y))).Single(p => grid[p.y][p.x] == 'S');
grid[start.y][start.x] = neighbours.Keys.Single(c => neighbours[c](start).All(e => neighbours[grid[e.y][e.x]](e).Any(p => p == start)));

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

var part2 =  Enumerable.Range(0,H).Select(y => 
                    String.Join("",Enumerable.Range(0,W).Select(x => visited.Contains(new Point(x,y)) ? grid[y][x] : '.'))
                    .Replace("-","")
                    .Replace("F7","")
                    .Replace("LJ","")
                    .Replace("L7", "|")
                    .Replace("FJ", "|"))
        .Sum(line => 
            line.Aggregate<char, (bool inside, int count), int>((false, 0), 
                (acc, c) => c == '|' 
                    ? (!acc.inside, acc.count) 
                    : (acc.inside, acc.inside ? acc.count + 1 : acc.count), acc => acc.count));

Console.WriteLine($"Part2: {part2} in {watch.ElapsedMilliseconds}ms");
record struct Point(int x, int y);

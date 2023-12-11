const long EXPANSION = 1000000;
var file = File.ReadAllLines("input.txt");
var watch = new System.Diagnostics.Stopwatch();
watch.Start();

var points = Enumerable.Range(0, file.Length).SelectMany(y => Enumerable.Range(0, file.First().Length).Select(x => new Point(x, y)).Where(p => file[p.y][p.x] == '#')).ToList();
var fullRows = new HashSet<int>(points.Select(p => p.y).Distinct());
var fullColumns = new HashSet<int>(points.Select(p => p.x).Distinct());

var answer = Enumerable.Range(0, points.Count).Sum(p1 => Enumerable.Range(p1 + 1, points.Count - p1 - 1).Sum(p2 =>
    Enumerable.Range(Math.Min(points[p1].x, points[p2].x), Math.Abs(points[p1].x - points[p2].x)).Sum(x => !fullColumns.Contains(x) ? EXPANSION : 1L)
    + Enumerable.Range(Math.Min(points[p1].y, points[p2].y), Math.Abs(points[p1].y - points[p2].y)).Sum(y => !fullRows.Contains(y) ? EXPANSION : 1L)));

Console.WriteLine($"Distance {answer} for expansion {EXPANSION} in {watch.ElapsedMilliseconds}ms");
record struct Point(int x, int y);

const long EXPANSION = 1000000;
var file = File.ReadAllLines("input.txt");

(int W, int H) = (file.First().Length, file.Length);
var points = Enumerable.Range(0, H).SelectMany(y => Enumerable.Range(0, W).Select(x => new Point(x, y)).Where(p => file[p.y][p.x] == '#')).ToList();
var emptyRows = new HashSet<int>(Enumerable.Range(0, H).Where(y => Enumerable.Range(0, W).All(x => file[y][x] == '.')));
var emptyCols = new HashSet<int>(Enumerable.Range(0, W).Where(x => Enumerable.Range(0, H).All(y => file[y][x] == '.')));

var answer = Enumerable.Range(0, points.Count).Sum(p1 => Enumerable.Range(p1 + 1, points.Count - p1 - 1).Sum(p2 =>
    Enumerable.Range(Math.Min(points[p1].x, points[p2].x), Math.Abs(points[p1].x - points[p2].x)).Sum(x => emptyCols.Contains(x) ? EXPANSION : 1L)
    + Enumerable.Range(Math.Min(points[p1].y, points[p2].y), Math.Abs(points[p1].y - points[p2].y)).Sum(y => emptyRows.Contains(y) ? EXPANSION : 1L)));

Console.WriteLine($"Part1: {answer}");
record struct Point(int x, int y);

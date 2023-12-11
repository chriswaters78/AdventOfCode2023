const int EXSPANSION = 1000000;
var file = File.ReadAllLines("input.txt");

(int W, int H) = (file.First().Length, file.Length);

var points = new List<Point>();
for (int y = 0; y < H; y++)
    for (int x = 0; x < W; x++)
        if (file[y][x] == '#')
            points.Add(new Point(x, y));

var emptyRows = new HashSet<int>(Enumerable.Range(0, H).Where(y => Enumerable.Range(0, W).All(x => file[y][x] == '.')));
var emptyCols = new HashSet<int>(Enumerable.Range(0, W).Where(x => Enumerable.Range(0, H).All(y => file[y][x] == '.')));

long totalD = 0;
for (int p1 = 0; p1 < points.Count; p1++)
    for (int p2 = p1+1; p2 < points.Count; p2++)
    {
        for (int xd = Math.Min(points[p1].x, points[p2].x); xd < Math.Max(points[p1].x, points[p2].x); xd++)
            totalD += emptyCols.Contains(xd) ? EXSPANSION : 1;
        for (int yd = Math.Min(points[p1].y, points[p2].y); yd < Math.Max(points[p1].y, points[p2].y); yd++)
            totalD += emptyRows.Contains(yd) ? EXSPANSION : 1;
    }

Console.WriteLine($"Part1: {totalD}");
record struct Point(int x, int y);

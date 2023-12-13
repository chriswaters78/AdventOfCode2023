System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
watch.Start();

var lines = File.ReadAllLines("input.txt");
var byRow = new List<List<char[]>>();
var current = new List<char[]>();
foreach (var line in lines)
{
    if (String.IsNullOrEmpty(line))
    {
        byRow.Add(current);
        current = new List<char[]>();
        continue;
    }
    current.Add(line.ToArray());
}
byRow.Add(current);

var byColumn = new List<List<char[]>>();
foreach (var gridR in byRow)
{
    var gridC = new List<char[]>();
    for (int c = 0; c < gridR.First().Length; c++)
    {
        var currC = "";
        for (int r = 0; r < gridR.Count; r++)
        {
            currC = $"{currC}{gridR[r][c]}";
        }
        gridC.Add(currC.ToArray());
    }
    byColumn.Add(gridC);
}



long part1 = 0;
long part2 = 0;
for (int i = 0; i < byColumn.Count; i++)
{
    var hR1 = findReflections(byRow[i]);
    var vR1 = findReflections(byColumn[i]);

    var originalR = hR1.Select(i => (Reflection.Horizontal, i)).Concat(vR1.Select(i => (Reflection.Vertical, i))).ToList();

    part1 += 100 * hR1.Sum() + vR1.Sum();

    //try flipping every bit
    for (int r = 0; r < byRow[i].Count; r++)
    {
        for (int c = 0; c < byRow[i][0].Length; c++)
        {
            byRow[i][r][c] = byRow[i][r][c] == '.' ? '#' : '.';
            byColumn[i][c][r] = byColumn[i][c][r] == '.' ? '#' : '.';

            var hR2 = findReflections(byRow[i]);
            var vR2 = findReflections(byColumn[i]);

            var newR = hR2.Select(i => (Reflection.Horizontal, i)).Concat(vR2.Select(i => (Reflection.Vertical, i)))
                .Except(originalR).ToList();

            if (newR.Count == 1)
            {
                part2 += 100 * newR.Where(tp => tp.Item1 == Reflection.Horizontal).Sum(tp => tp.i) 
                    + newR.Where(tp => tp.Item1 == Reflection.Vertical).Sum(tp => tp.i);
                goto next;          
            }

            byRow[i][r][c] = byRow[i][r][c] == '.' ? '#' : '.';
            byColumn[i][c][r] = byColumn[i][c][r] == '.' ? '#' : '.';
        }
    }
next:;
}

Console.WriteLine($"Part1: {part1}");
Console.WriteLine($"Part2: {part2}");
Console.WriteLine($"Elapsed time: {watch.ElapsedMilliseconds}ms");

List<int> findReflections (List<char[]> grid) => Enumerable.Range(1, grid.Count - 1)
        .Where(r => grid.Take(r).Reverse().Zip(grid.Skip(r))
        .All(tp => tp.First.SequenceEqual(tp.Second))).ToList();

void printGrid(List<char[]> grid)
{
    foreach (var line in grid)
    {
        Console.WriteLine(String.Join("",line));
    }
    Console.WriteLine();
}

enum Reflection
{
    Horizontal,
    Vertical
}
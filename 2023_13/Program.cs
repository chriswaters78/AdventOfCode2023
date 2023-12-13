var lines = File.ReadAllLines("input.txt");
List<List<string>> byRow = new List<List<string>>();

var current = new List<string>();
foreach (var line in lines)
{
    if (String.IsNullOrEmpty(line))
    {
        byRow.Add(current);
        current = new List<string>();
        continue;
    }
    current.Add(line);
}
byRow.Add(current);

List<List<string>> byColumn = new List<List<string>>();
foreach (var gridR in byRow)
{
    var gridC = new List<string>();
    for (int c = 0; c < gridR.First().Length; c++)
    {
        var currC = "";
        for (int r = 0; r < gridR.Count; r++)
        {
            currC = $"{currC}{gridR[r][c]}";
        }
        gridC.Add(currC);
    }
    byColumn.Add(gridC);
}

printGrid(byRow.First());
Console.WriteLine();
printGrid(byColumn.First());

long part1 = 0;
for (int i = 0; i < byColumn.Count; i++)
{
    var hReflections = Enumerable.Range(1, byRow[i].Count - 1)
        .Where(r => byRow[i].Take(r).Reverse().Zip(byRow[i].Skip(r))
        .All(tp => tp.First == tp.Second)).ToList();

    var vReflections = Enumerable.Range(1, byColumn[i].Count - 1)
        .Where(r => byColumn[i].Take(r).Reverse().Zip(byColumn[i].Skip(r))
        .All(tp => tp.First == tp.Second)).ToList();

    part1 += 100 * hReflections.Sum() + vReflections.Sum();
}

Console.WriteLine($"Part1: {part1}");

void printGrid(List<string> grid)
{
    foreach (var line in grid)
    {
        Console.WriteLine(line);
    }
}
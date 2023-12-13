var byRow = File.ReadAllText("input.txt").Split($"{Environment.NewLine}{Environment.NewLine}")
    .Select(grid => grid.Split($"{Environment.NewLine}")).ToArray();

var byColumn = byRow.Select(grid =>
    Enumerable.Range(0, grid[0].Length).Select(c => String.Join("", 
            Enumerable.Range(0, grid.Length).Select(r => grid[r][c]))).ToArray()).ToArray();

(var part1, var part2) = byColumn.Zip(byRow).Select(tp =>
{
    (var hR, var vR) = (findReflectionSmudges(tp.Second), findReflectionSmudges(tp.First));    
    (var hR0, var vR0) = (filter(hR,0), filter(vR, 0));
    (var hR1, var vR1) = (filter(hR, 1).Except(hR0), filter(vR, 1).Except(vR0));

    return (100 * hR0.Sum() + vR0.Sum(), 100 * hR1.Sum() + vR1.Sum());
}).Aggregate((tp1, tp2) => (tp1.Item1 + tp2.Item1, tp1.Item2 + tp2.Item2));

Console.WriteLine($"Part1: {part1}");
Console.WriteLine($"Part2: {part2}");

List<(int i, int s)> findReflectionSmudges(string[] grid) =>
    Enumerable.Range(1, grid.Length - 1)
        .Select(r => (r, grid.Take(r).Reverse().Zip(grid.Skip(r))
            .Sum(tp => tp.First.Zip(tp.Second).Count(tp => tp.First != tp.Second)))).ToList();

List<int> filter(List<(int i, int s)> reflections, int smudges) => reflections.Where(tp => tp.s == smudges).Select(tp => tp.i).ToList();

void printGrid(string[] grid) => grid.Concat(new[] { "" }).ToList().ForEach(Console.WriteLine);

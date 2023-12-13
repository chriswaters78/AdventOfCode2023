var byRow = File.ReadAllText("input.txt").Split($"{Environment.NewLine}{Environment.NewLine}")
    .Select(grid => grid.Split($"{Environment.NewLine}")).ToArray();

var byColumn = byRow.Select(grid =>
    Enumerable.Range(0, grid[0].Length).Select(c => String.Join("", 
            Enumerable.Range(0, grid.Length).Select(r => grid[r][c]))).ToArray()).ToArray();

var answers =
    from tp in byColumn.Zip(byRow)
    let hR = findReflectionSmudges(tp.Second)
    let vR = findReflectionSmudges(tp.First)
    let hR0 = filter(hR, 0)
    let vR0 = filter(vR, 0)
    let hR1 = filter(hR, 1).Except(hR0)
    let vR1 = filter(vR, 1).Except(vR0)
    select (100 * hR0.Sum() + vR0.Sum(), 100 * hR1.Sum() + vR1.Sum());

Console.WriteLine($"Part1: {answers.Sum(tp => tp.Item1)}");
Console.WriteLine($"Part2: {answers.Sum(tp => tp.Item2)}");

List<(int i, int s)> findReflectionSmudges(string[] grid) =>
    Enumerable.Range(1, grid.Length - 1)
        .Select(r => (r, grid.Take(r).Reverse().Zip(grid.Skip(r))
            .Sum(tp => tp.First.Zip(tp.Second).Count(tp => tp.First != tp.Second)))).ToList();

List<int> filter(List<(int i, int s)> reflections, int smudges) => reflections.Where(tp => tp.s == smudges).Select(tp => tp.i).ToList();

const long CYCLES = 1000000000L;

var grid = File.ReadAllLines("test.txt").Select(str => str.ToArray()).ToList();
(int R, int C) = (grid.Count, grid[0].Length);

var grid1 = grid.Select(arr => arr.ToArray()).ToList();
shuffle(grid1,0);
var part1 = score(grid1);

Console.WriteLine($"Part1: {part1}");

var gridCache = new Dictionary<string, long>();
var grid2 = grid.Select(arr => arr.ToArray()).ToList();
for (long i = 0; i < CYCLES; i++)
{
    for (int dir = 0; dir < 4; dir++)
        shuffle(grid2, dir);

    var key = print(grid2);
    if (gridCache.TryGetValue(key, out long cycleStart))
    {
        var cycleLength = i - cycleStart;
        var skip = (CYCLES - i) / cycleLength;
        i += skip * cycleLength;
        if (skip > 0) Console.WriteLine($"cycleLength:{cycleLength}, cycleStart:{cycleStart}, skip {skip} to {i}");
    }
    gridCache[key] = i;
}

var part2 = score(grid2);
Console.WriteLine($"Part2: {part2}");

long score(List<char[]> grid) => 
    Enumerable.Range(0, R).Sum(r => 
        Enumerable.Range(0, C).Sum(c => grid[r][c] == 'O' ? R - r : 0));

void shuffle(List<char[]> grid, int direction)
{
    (bool swap, bool reverse) = (direction % 2 == 1, direction % 4 >= 2);
    (var outer, var inner) = swap ? (R, C) : (C, R);
    for (int i1 = 0; i1 < outer; i1++)
    {
        bool anyMoved;
        do
        {
            anyMoved = false;
            for (int i2 = 1; i2 < inner; i2++)
            {
                (var i2O, var i2E) = reverse ? (inner - i2 - 1, inner - i2) : (i2, i2 - 1);
                (var rO, var cO, var rE, var cE) = swap ? (i1, i2O, i1, i2E) : (i2O,i1, i2E, i1);
                
                if (grid[rE][cE] == '.' && grid[rO][cO] == 'O')
                {
                    grid[rE][cE] = 'O';
                    grid[rO][cO] = '.';
                    anyMoved = true;
                }
            }
        } while (anyMoved);
    }
}

string print(List<char[]> grid) => grid.Aggregate(new System.Text.StringBuilder(), (sb, arr) => sb.AppendLine(new (arr)), sb => sb.ToString());

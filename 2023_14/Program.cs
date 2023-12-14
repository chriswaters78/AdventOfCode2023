using System.Text;

var grid = File.ReadAllLines("input.txt").Select(str => str.ToArray()).ToList();
(int R, int C) = (grid.Count, grid[0].Length);

var grid1 = grid.Select(arr => arr.ToArray()).ToList();
shuffle(grid1,0);

var part1 = score(grid1);
Console.WriteLine($"Part1: {part1}");

const long cycles = 1000000000L;

var grids = new Dictionary<string, long>();

(long r1, long r2) = (-1, -1);
var grid2 = grid.Select(arr => arr.ToArray()).ToList();
for (long i = 0; i < cycles; i++)
{
    shuffle(grid2, 0);
    shuffle(grid2, 1);
    shuffle(grid2, 2);
    shuffle(grid2, 3);

    Console.WriteLine($"After {i} Score {score(grid2)}");
    var key = print(grid2);
    if (grids.ContainsKey(key))
    {
        (r1, r2) = (grids[key], i);
        var inc = (cycles - r2) / (r2 - r1);
        i += inc * (r2 - r1);
    }
    grids[key] = i;
}

var part2 = score(grid2);
Console.WriteLine($"Part2: {part2}");

long score(List<char[]> grid)
{
    var answer = 0L;
    for (int r = 0; r < R; r++)
    {
        for (int c = 0; c < C; c++)
        {
            if (grid[r][c] == 'O')
            {
                answer += (R - r);
            }
        }
    }

    return answer;
}
void shuffle(List<char[]> grid, int direction)
{
    if (direction == 0)
    {
        for (int c = 0; c < C; c++)
        {
            bool anyMoved;
            do
            {
                anyMoved = false;
                for (int r = 1; r < R; r++)
                {
                    if (grid[r - 1][c] == '.' && grid[r][c] == 'O')
                    {
                        grid[r - 1][c] = 'O';
                        grid[r][c] = '.';
                        anyMoved = true;
                    }
                }
            } while (anyMoved);
        }
    }
    else if (direction == 1)
    {
        for (int r = 0; r < R; r++)
        {
            bool anyMoved;
            do
            {
                anyMoved = false;
                for (int c = 1; c < C; c++)
                {
                    if (grid[r][c - 1] == '.' && grid[r][c] == 'O')
                    {
                        grid[r][c - 1] = 'O';
                        grid[r][c] = '.';
                        anyMoved = true;
                    }
                }
            } while (anyMoved);
        }
    }
    else if (direction == 2)
    {
        for (int c = 0; c < C; c++)
        {
            bool anyMoved;
            do
            {
                anyMoved = false;
                for (int r = R - 2; r >= 0; r--)
                {
                    if (grid[r + 1][c] == '.' && grid[r][c] == 'O')
                    {
                        grid[r + 1][c] = 'O';
                        grid[r][c] = '.';
                        anyMoved = true;
                    }
                }
            } while (anyMoved);
        }
    }
    else
    {
        for (int r = 0; r < R; r++)
        {
            bool anyMoved;
            do
            {
                anyMoved = false;
                for (int c = C - 2; c >= 0; c--)
                {
                    if (grid[r][c + 1] == '.' && grid[r][c] == 'O')
                    {
                        grid[r][c + 1] = 'O';
                        grid[r][c] = '.';
                        anyMoved = true;
                    }
                }
            } while (anyMoved);
        }
    }
}

string print(List<char[]> grid)
{
    var builder = new StringBuilder(); 
    foreach (var arr in grid)
    {
        builder.AppendLine(String.Join("", arr));
    }
    return builder.ToString();
}

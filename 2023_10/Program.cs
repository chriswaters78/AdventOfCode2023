namespace _2023_10
{
    internal class Program
    {
        static Dictionary<char, List<Offset>> offsetMap = null;
        static void Main(string[] args)
        {
            var file = File.ReadAllLines("input.txt");
            char[][] grid = new char[file.Length + 2][];
            grid[0] = Enumerable.Repeat('.', file.First().Length + 2).ToArray();
            foreach (var (line, i) in file.Select((line, i) => (line, i)))
            {
                grid[i + 1] = new[] { '.' }.Concat(line).Concat(new[] { '.' }).ToArray();
            }
            grid[grid.Length - 1] = Enumerable.Repeat('.', file.First().Length + 2).ToArray();

            offsetMap = new Dictionary<char, List<Offset>>()
            {
                { '|', new List<Offset>() { new (0,-1), new (0,1) } },
                { '-', new List<Offset>() { new (-1,0), new (1,0) } },
                { 'L', new List<Offset>() { new (0,-1), new (1,0) } },
                { 'J', new List<Offset>() { new (0,-1), new (-1,0) } },
                { '7', new List<Offset>() { new (-1,0), new (0,1) } },
                { 'F', new List<Offset>() { new (0,1), new (1,0) } },
                { '.', new List<Offset>() { } },
                { 'S', new List<Offset>() { new(-1, 0), new(1, 0), new(0, -1), new(0, 1) } },
            };

            Point start = new (-1,-1);
            for (int y = 0; y < grid.Length; y++)
            {
                for (int x = 0; x < grid[y].Length; x++)
                {
                    if (grid[y][x] == 'S')
                    {
                        start = new (x, y);
                        break;
                    }
                }
            }

            var initialStack = new Stack<StepResult>();
            initialStack.Push(new StepResult(start, 0, new HashSet<Path>() { }));
            var part1Results = step(initialStack, grid);

            var part1 = Math.Ceiling((decimal) part1Results.steps / 2m);

            var taken = new HashSet<Point>(part1Results.taken.Select(p => p.point));

            var neighbour1 = part1Results.taken.Single(p => start == new Point(p.point.x - p.offset.ox, p.point.y - p.offset.oy)).point;
            var lastMove = part1Results.taken.Single(p => p.point == start);
            var neighbour2 = new Point(start.x - lastMove.offset.ox, start.y - lastMove.offset.oy);

            //always seems to be F, which is nice!
            grid[start.y][start.x] = 'F';

            var withinLoop = new List<Point>();
            for (int y = 0; y < grid.Length; y++)
            {
                for (int x = 0; x < grid[y].Length; x++)
                {
                    //we never start on the route, only on potentially interior places
                    if (taken.Contains(new Point(x, y)))
                    {
                        continue;
                    }
                    int crossings = 0;
                    char entered = '.';
                    for (int lx = x - 1; lx >= 0; lx--)
                    {
                        var current = !taken.Contains(new Point(lx,y)) ? '.' : grid[y][lx];
                        //we cross from lx + 1 to lx
                        //have we fully crossed a pipe?
                        switch ((entered,current))
                        {
                            case ('.', '7'):
                                entered = '7';
                                break;
                            case ('.', 'J'):
                                entered = 'J';
                                break;
                            case ('7', 'L'):
                                crossings++;
                                entered = '.';
                                break;
                            case ('7', 'F'):
                                entered = '.';
                                break;
                            case ('J', 'F'):
                                crossings++;
                                entered = '.';
                                break;
                            case ('J', 'L'):
                                entered = '.';
                                break;
                            case (_, '-'):
                                break;
                            case ('.', '|'):
                                crossings++;
                                break;
                            case ('.', '.'):
                                break;
                            default:
                                throw new();
                        }
                    }
                    if (crossings % 2 == 1)
                    {
                        withinLoop.Add(new Point(x, y));
                    }
                }
            }

            Console.WriteLine($"Part2: {withinLoop.Count}");
        }
        private static StepResult step(Stack<StepResult> stack, char[][] grid)
        {
            //we are at result, find any valid connections
            //and finish when we get to S again
            while (true)
            {
                if (stack.Any())
                {
                    var result = stack.Pop();
                    foreach (var offset in offsetMap[grid[result.current.y][result.current.x]])
                    {
                        if (result.taken.Contains(new Path(result.current, new Offset(-offset.ox, -offset.oy))))
                        {
                            continue;
                        }
                        var next = new Point(result.current.x + offset.ox, result.current.y + offset.oy);
                        foreach (var offsetDest in offsetMap[grid[next.y][next.x]])
                        {
                            //if the dest joins back to where we are coming from
                            if (offset.oy == -offsetDest.oy && offset.ox == -offsetDest.ox)
                            {
                                var newTaken = new HashSet<Path>(result.taken) { new Path(next, offset) };
                                var newStepResult = result with { current = next, steps = result.steps + 1, taken = newTaken };
                                //this is a valid direction to move
                                if (grid[next.y][next.x] == 'S')
                                {
                                    //we have made a full loop, return this result
                                    return newStepResult;
                                }
                                else
                                {
                                    //keep going!
                                    stack.Push(newStepResult);
                                }
                            }
                        }
                    }
                }
            }
            throw new();
        }

        record struct Point(int x, int y);
        record struct Offset(int ox, int oy);
        record struct Path(Point point,Offset offset);
        record struct StepResult(Point current, int steps, HashSet<Path> taken);
    }
}

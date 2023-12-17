﻿var file = File.ReadAllLines("input.txt");
(int R, int C) = (file.Length, file.First().Length);

var grid = Enumerable.Range(0, R).SelectMany(r => Enumerable.Range(0, C).Select(c => (new Point(r, c), int.Parse($"{file[r][c]}"))))
    .ToDictionary(tp => tp.Item1, tp => tp.Item2);

var part1 = solve(0, 3);
Console.WriteLine($"Part1: {part1}");

var part2 = solve(4, 10);
Console.WriteLine($"Part2: {part2}");

int solve(int minMoves, int maxMoves)
{
    var cache = new Dictionary<State, int>();

    var queue = new PriorityQueue<State, int>();
    queue.Enqueue(new State(new Point(0, 0), Direction.East, 0), 0);
    queue.Enqueue(new State(new Point(0, 0), Direction.South, 0), 0);

    while (true)
    {
        State currS;
        int currP;
        if (!queue.TryDequeue(out currS, out currP))
        {
            break;
        }

        if (cache.ContainsKey(currS) && cache[currS] <= currP)
        {
            //we have done better
            continue;
        }
        cache[currS] = currP;

        foreach (var dirE in validDirections(currS.lastMove))
        {
            var dir = (int)dirE;
            if ((int)currS.lastMove != dir && currS.movesInSameDirection < minMoves)
            {
                continue;
            }
            if ((int)currS.lastMove == dir && currS.movesInSameDirection >= maxMoves)
            {
                continue;
            }
            //we can move this way
            var newState = new State(move(currS.point, dir), (Direction)dir, (int)currS.lastMove == dir ? currS.movesInSameDirection + 1 : 1);

            if (newState.point.r < 0 || newState.point.c < 0 || newState.point.r >= R || newState.point.c >= C)
            {
                continue;
            }
            var newPriority = currP + grid[newState.point];
            queue.Enqueue(newState, newPriority);
        }
    }
    return cache.Where(kvp => kvp.Key.point == new Point(R - 1, C - 1)).MinBy(kvp => kvp.Value).Value;
}

Point move(Point p, int d) => d switch
{
    0 => new Point(p.r - 1, p.c),
    1 => new Point(p.r, p.c + 1),
    2 => new Point(p.r + 1, p.c),
    3 => new Point(p.r, p.c - 1),
};

List<Direction> validDirections(Direction dir) => dir switch
{
    Direction.North => new List<Direction>() { Direction.North, Direction.West, Direction.East },
    Direction.East => new List<Direction>() { Direction.East, Direction.North, Direction.South },
    Direction.South => new List<Direction>() { Direction.South, Direction.West, Direction.East },
    Direction.West => new List<Direction>() { Direction.West, Direction.North, Direction.South },
};

record struct State(Point point, Direction lastMove, int movesInSameDirection);
record struct Point(int r, int c);

enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3,
}

var grid = File.ReadAllLines("input.txt");
(int R, int C) = (grid.Length, grid.First().Length);

var part1 = new[] { new Beam(new Point(0, 0), Direction.East), new Beam(new Point(0, 0), Direction.South) }.Max(solve);
Console.WriteLine($"Part1: {part1}");

var part2 = Enumerable.Range(0, R)
    .SelectMany(r => new[] { new Beam(new Point(r, 0), Direction.East), new Beam(new Point(r, C - 1), Direction.West) })
    .Concat(Enumerable.Range(0, C).SelectMany(c => new[] { new Beam(new Point(0, c), Direction.South), new Beam(new Point(R - 1, c), Direction.North) }))
    .Max(solve);

Console.WriteLine($"Part2: {part2}");

int solve(Beam initialBeam)
{
    var beams = new Queue<Beam>();
    beams.Enqueue(initialBeam);

    var energised = Enumerable.Range(0, R).SelectMany(r => Enumerable.Range(0, C).Select(c => new Point(r, c))).ToDictionary(p => p, _ => 0);

    while (beams.Any())
    {
        var beam = beams.Dequeue();

        if (beam.point.r < 0 || beam.point.c < 0 || beam.point.r >= R || beam.point.c >= C)
            continue;

        //if we have already had a beam pass through here in the same direction, we can stop
        if ((energised[beam.point] & (int)beam.dir) > 0)
            continue;

        //mark that we have passed through this point in this direction
        energised[beam.point] = energised[beam.point] | (int)beam.dir;

        //add the any resulting beams to our queue
        switch ((beam.dir, grid[beam.point.r][beam.point.c]))
        {
            case (_, '.'):
            case (Direction.North, '|'):
            case (Direction.South, '|'):
            case (Direction.West, '-'):
            case (Direction.East, '-'):
                beams.Enqueue(next(beam.point, beam.dir));
                break;
            case (Direction.North, '/'):
            case (Direction.South, '\\'):
                beams.Enqueue(next(beam.point, Direction.East));
                break;
            case (Direction.East, '/'):
            case (Direction.West, '\\'):
                beams.Enqueue(next(beam.point, Direction.North));
                break;
            case (Direction.South, '/'):
            case (Direction.North, '\\'):
                beams.Enqueue(next(beam.point, Direction.West));
                break;
            case (Direction.West, '/'):
            case (Direction.East, '\\'):
                beams.Enqueue(next(beam.point, Direction.South));
                break;
            case (Direction.North, '-'):
            case (Direction.South, '-'):
                beams.Enqueue(next(beam.point, Direction.West));
                beams.Enqueue(next(beam.point, Direction.East));
                break;
            case (Direction.West, '|'):
            case (Direction.East, '|'):
                beams.Enqueue(next(beam.point, Direction.North));
                beams.Enqueue(next(beam.point, Direction.South));
                break;
        }
    }

    return energised.Where(kvp => kvp.Value != 0).Count();
}

string printEnergised(Dictionary<Point, int> energised)
{
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    for (int r = 0; r < R; r++)
    {
        for (int c = 0; c < C; c++)
        {
            var directions = Enumerable.Range(0, 4).Where(dir => (energised[new Point(r, c)] & dir) != 0).ToList();
            sb.Append(directions switch
            {
                [] => '.',
                [var dir] => dir switch {
                    (int)Direction.North => '^',
                    (int)Direction.East => 'v',
                    (int)Direction.South => '>',
                    (int)Direction.West => '<',
                },
                _ => directions.Count
            });
        }
        sb.AppendLine();
    }
    return sb.ToString();
}

Beam next(Point point, Direction dir) => dir switch
{
    Direction.North => new Beam(new Point(point.r - 1, point.c), dir),
    Direction.East => new Beam(new Point(point.r, point.c + 1), dir),
    Direction.South => new Beam(new Point(point.r + 1, point.c), dir),
    Direction.West => new Beam(new Point(point.r, point.c - 1), dir),
};

record struct Beam(Point point, Direction dir);
record struct Point(int r, int c);

[Flags]
enum Direction
{
    North = 1,
    East = 2,
    South = 4,
    West = 8,
}

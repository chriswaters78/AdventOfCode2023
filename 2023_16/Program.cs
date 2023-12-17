var grid = File.ReadAllLines("input.txt");

(int R, int C) = (grid.Length, grid.First().Length);

{
    var beams = new Queue<Beam>();
    beams.Enqueue(new Beam(new Point(0, 0), Direction.East));
    var energised = solve(beams);

    //Console.WriteLine(printEnergised(energised));
    var part1 = energised.Where(kvp => kvp.Value != 0).Count();
    Console.WriteLine($"Part1: {part1}");
}

var part2 = -1;
for (int r = 0; r < R; r++)
{
    var beams = new Queue<Beam>();
    beams.Enqueue(new Beam(new Point(r, 0), Direction.East));
    var energised = solve(beams);
    part2 = Math.Max(part2, energised.Where(kvp => kvp.Value != 0).Count());
    
    beams.Enqueue(new Beam(new Point(r, C - 1), Direction.West));
    energised = solve(beams);
    part2 = Math.Max(part2, energised.Where(kvp => kvp.Value != 0).Count());
}
for (int c = 0; c < C; c++)
{
    var beams = new Queue<Beam>();
    beams.Enqueue(new Beam(new Point(0, c), Direction.South));
    var energised = solve(beams);
    part2 = Math.Max(part2, energised.Where(kvp => kvp.Value != 0).Count());

    beams.Enqueue(new Beam(new Point(R - 1, c), Direction.North));
    energised = solve(beams);
    part2 = Math.Max(part2, energised.Where(kvp => kvp.Value != 0).Count());
}

Console.WriteLine($"Part2: {part2}");

Dictionary<Point, int> solve(Queue<Beam> beams)
{
    var energised = Enumerable.Range(0, R).SelectMany(r => Enumerable.Range(0, C).Select(c => new Point(r, c))).ToDictionary(p => p, _ => 0);

    while (beams.Any())
    {
        var beam = beams.Dequeue();

        if (beam.point.r < 0 || beam.point.c < 0 || beam.point.r >= R || beam.point.c >= C)
            continue;

        if ((energised[beam.point] & (int)beam.dir) > 0)
            continue;

        energised[beam.point] = energised[beam.point] | (int)beam.dir;

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
                beams.Enqueue(next(beam.point, Direction.East));
                break;
            case (Direction.East, '/'):
                beams.Enqueue(next(beam.point, Direction.North));
                break;
            case (Direction.South, '/'):
                beams.Enqueue(next(beam.point, Direction.West));
                break;
            case (Direction.West, '/'):
                beams.Enqueue(next(beam.point, Direction.South));
                break;
            case (Direction.North, '\\'):
                beams.Enqueue(next(beam.point, Direction.West));
                break;
            case (Direction.East, '\\'):
                beams.Enqueue(next(beam.point, Direction.South));
                break;
            case (Direction.South, '\\'):
                beams.Enqueue(next(beam.point, Direction.East));
                break;
            case (Direction.West, '\\'):
                beams.Enqueue(next(beam.point, Direction.North));
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

    return energised;
}

string printEnergised(Dictionary<Point, int> energised)
{
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    for (int r = 0; r < R; r++)
    {
        for (int c = 0; c < C; c++)
        {
            var point = new Point(r, c);
            var directions = new List<Direction>();
            if ((energised[point] & (int)Direction.North) != 0)
                directions.Add(Direction.North);
            if ((energised[point] & (int)Direction.East) != 0)
                directions.Add(Direction.East);
            if ((energised[point] & (int)Direction.South) != 0)
                directions.Add(Direction.South);
            if ((energised[point] & (int)Direction.West) != 0)
                directions.Add(Direction.West);

            if (directions.Count == 0)
            {
                sb.Append('.');
            }
            else if (directions.Count == 1)
            {
                sb.Append(directions.First() switch {
                    Direction.North => '^',
                    Direction.South => 'v',
                    Direction.East => '>',
                    Direction.West => '<',
                });
            }
            else
            {
                sb.Append(directions.Count);
            }
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

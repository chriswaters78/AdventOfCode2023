var bricks = File.ReadAllLines("input.txt").Select(line =>
{
    var sp = line.Split("~").Select(str => str.Split(",").Select(int.Parse).ToArray()).ToArray();
    return (new Point3(Math.Min(sp[0][0], sp[1][0]), Math.Min(sp[0][1], sp[1][1]), Math.Min(sp[0][2], sp[1][2])), new Point3(Math.Max(sp[0][0], sp[1][0]), Math.Max(sp[0][1], sp[1][1]), Math.Max(sp[0][2], sp[1][2])));
}).OrderByDescending(tp => tp.Item1.z).ToList();

(int maxx, int maxy) = (bricks.Max(tp => Math.Max(tp.Item1.x, tp.Item2.x)), bricks.Max(tp => Math.Max(tp.Item1.y, tp.Item2.y)));

Console.WriteLine($"maxx: {maxx}, maxy: {maxy}");


(var newBrickPositions, var moved) = collapse(bricks);

Console.WriteLine($"{moved} bricks moved after initial collapse");

var supports = Enumerable.Range(0, newBrickPositions.Count).ToDictionary(i => i, _ => new HashSet<int>());
for (var b1 = 0; b1 < newBrickPositions.Count; b1++)
{
    for (var b2 = 0; b2 < newBrickPositions.Count; b2++)
    {
        if (b1 == b2)
            continue;

        var brick1 = newBrickPositions[b1];
        var brick2 = newBrickPositions[b2];
        int supporting = 0;
        //do b1 and b2 overlap?
        if (brick1.Item2.z == brick2.Item1.z - 1)
        {
            //b1 is directly underneath b2
            //if they overlap then b1 is supporting b2
            //| --- |
            //   |-----|
            if (brick1.Item1.x <= brick2.Item2.x && brick1.Item2.x >= brick2.Item1.x
                && brick1.Item1.y <= brick2.Item2.y && brick1.Item2.y >= brick2.Item1.y)
            {
                supports[b1].Add(b2);
            }
        }
    }
}

var supportedBy = supports.SelectMany(kvp => kvp.Value.Select(supported => (kvp.Key, supported)))
    .GroupBy(tp => tp.supported)
    .ToDictionary(grp => grp.Key, grp => grp.ToHashSet());

int part1 = 0;
//we can remove any brick, where all the bricks it supports are supported by at least one other
for (int i = 0; i < newBrickPositions.Count; i++)
{
    if (supports[i].All(supported => supportedBy[supported].Count > 1))
    {
        part1++;
    }
}

Console.WriteLine($"Part1: {part1}");

(var again, var againMoved) = collapse(newBrickPositions);


//try removing each brick in turn, how many bricks fall?
int part2 = 0;
for (int i = 0; i < newBrickPositions.Count; i++)
{
    var afterRemove = newBrickPositions.ToList();
    afterRemove.RemoveAt(i);

    (var afterCollapse, var moved2) = collapse(afterRemove);
    Console.WriteLine($"Removing brick {i} caused {moved2} moves");
    part2 += moved2;
}

Console.WriteLine($"Part2: {part2}");


(List<(Point3, Point3)>, int) collapse(List<(Point3, Point3)> bricks)
{
    int moved = 0;
    var heightMap = Enumerable.Range(0, maxx + 1).SelectMany(x => Enumerable.Range(0, maxy + 1).Select(y => new Point2(x, y)))
    .ToDictionary(p => p, _ => 0);

    var newBrickPositions = new List<(Point3, Point3)>();
    for (int i = bricks.Count - 1; i >= 0; i--)
    {
        //consider the last brick in the list (the lowest by z)
        int maxHeight = 0;
        for (int x = bricks[i].Item1.x; x <= bricks[i].Item2.x; x++)
        {
            for (int y = bricks[i].Item1.y; y <= bricks[i].Item2.y; y++)
            {
                maxHeight = Math.Max(heightMap[new Point2(x, y)], maxHeight);
            }
        }
        //we have the maxheight in the x-y projection of the brick
        //we can place the brick at maxheight + 1, and this will cover
        if (maxHeight + 1 < bricks[i].Item1.z)
        {
            moved++;
        }
        var brickHeight = bricks[i].Item2.z - bricks[i].Item1.z;
        newBrickPositions.Add((bricks[i].Item1 with { z = maxHeight + 1 }, bricks[i].Item2 with { z = maxHeight + brickHeight + 1 }));

        for (int x = bricks[i].Item1.x; x <= bricks[i].Item2.x; x++)
        {
            for (int y = bricks[i].Item1.y; y <= bricks[i].Item2.y; y++)
            {
                heightMap[new Point2(x, y)] = maxHeight + brickHeight + 1;
            }
        }
    }

    return (newBrickPositions.OrderByDescending(tp => tp.Item1.z).ToList(), moved);
}


record struct Point2(int x, int y);
record struct Point3(int x, int y, int z);
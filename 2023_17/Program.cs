using System.Numerics;
System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
watch.Start();
var file = File.ReadAllLines("input.txt");
(int R, int C) = (file.Length, file.First().Length);

var grid = Enumerable.Range(0, R).SelectMany(
        r => Enumerable.Range(0, C).Select(
            c => (new Complex(r, c), int.Parse($"{file[r][c]}"))))
    .ToDictionary(tp => tp.Item1, tp => tp.Item2);

var part1 = solve(0, 3);
Console.WriteLine($"Part1: {part1} in {watch.ElapsedMilliseconds}ms");

var part2 = solve(4, 10);
Console.WriteLine($"Part2: {part2} in {watch.ElapsedMilliseconds}ms");

int solve(int minMoves, int maxMoves)
{
    var cache = new Dictionary<State, int>();

    var queue = new PriorityQueue<State, int>();
    queue.Enqueue(new State(new Complex(0, 0), new Complex(0,1), 0), 0);
    queue.Enqueue(new State(new Complex(0, 0), new Complex(-1, 0), 0), 0);

    while (queue.TryDequeue(out State state, out int cost))
    {
        if (cache.TryGetValue(state, out int cachedCost) && cachedCost <= cost)
            continue;
        
        cache[state] = cost;
        foreach (var dir in new[] { state.direction * -Complex.ImaginaryOne, state.direction, state.direction *  Complex.ImaginaryOne})
        {
            if (state.direction != dir && state.moves < minMoves
            || state.direction == dir && state.moves >= maxMoves)
                continue;

            var nextState = new State(state.point + dir, dir, state.direction == dir ? state.moves + 1 : 1);
            if (nextState.point.Real < 0 || nextState.point.Imaginary < 0 || nextState.point.Real >= R || nextState.point.Imaginary >= C)
                continue;

            queue.Enqueue(nextState, cost + grid[nextState.point]);
        }
    }
    return cache.Where(kvp => kvp.Key.point == new Complex(R - 1, C - 1)).MinBy(kvp => kvp.Value).Value;
}

record struct State(Complex point, Complex direction, int moves);



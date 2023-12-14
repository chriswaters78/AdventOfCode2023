System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
watch.Start();
var rows = File.ReadAllLines("input.txt").Select(line => line.Split(" "))
    .Select(sp => new Row( $"{sp[0]}", sp[1].Split(",").Select(int.Parse).ToList())).ToList();

var part1 = solve(rows);
Console.WriteLine($"Part1: {part1} in {watch.ElapsedMilliseconds}ms");

var part2 = solve(
    rows.Select(puzzle => new Row(
        $"{String.Join("?", Enumerable.Repeat(puzzle.condition, 5))}",
        Enumerable.Repeat(0, 5).SelectMany(_ => puzzle.spec).ToList()
)));

Console.WriteLine($"Part2: {part2} in {watch.ElapsedMilliseconds}ms");

long solve(IEnumerable<Row> rows) => rows.AsParallel().Select(row => consume(new State(0, 0, row.spec.Count), new(), $"{row.condition}.", row.spec)).Sum();
long consume(State state, Dictionary<State, long> stateCache, string condition, List<int> spec)
{
    if (stateCache.ContainsKey(state))
        return stateCache[state];
            
    if (state.consumed == condition.Length)
        return state.remainingRuns == 0 ? 1 : 0;

    var charCanBe = condition[state.consumed] == '?' ? ".#" : $"{condition[state.consumed]}";
    var result = 0L;
    foreach (var ch in charCanBe)
        result += ch switch
        {
            '.' when state.run == 0
                => consume(state with { consumed = state.consumed + 1 }, stateCache, condition, spec),
            '.' when state.remainingRuns >= 1 && state.run == spec[spec.Count - state.remainingRuns]
                => consume(state with { consumed = state.consumed + 1, run = 0, remainingRuns = state.remainingRuns - 1 }, stateCache, condition, spec),
            '.' => 0,
            '#' => consume(state with { consumed = state.consumed + 1, run = state.run + 1 }, stateCache, condition, spec)
        };

    return stateCache[state] = result;
}

record struct State(int consumed, int run, int remainingRuns);
record struct Row(string condition, List<int> spec);

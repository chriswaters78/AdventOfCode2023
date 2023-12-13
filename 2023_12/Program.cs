namespace _2023_12
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var puzzles = File.ReadAllLines("input.txt").Select(line => line.Split(" "))
                .Select(sp => new { masks = sp[0], specs = sp[1].Split(",").Select(int.Parse).ToList() }).ToList();

            Func<long> solve = () => puzzles.Select(a => consume(new State($"{a.masks}.", 0, a.specs), new())).Sum();
            Console.WriteLine($"Part1: {solve()}");

            puzzles = puzzles.Select(puzzle => new {
                masks = String.Join("?", Enumerable.Repeat(puzzle.masks, 5)),
                specs = Enumerable.Repeat(0, 5).SelectMany(_ => puzzle.specs).ToList()
            }).ToList();

            Console.WriteLine($"Part2: {solve()}");
        }

        static long consume(State state, Dictionary<string,long> stateCache)
        {
            var cacheKey = stateToCacheKey(state);
            if (stateCache.ContainsKey(cacheKey))
                return stateCache[cacheKey];
            
            if (state.remainingMask.Length == 0)
                return state.remainingSpec.Any() ? 0 : 1;

            var currMask = state.remainingMask[0] == '?' ? ".#" : state.remainingMask[0..1];
            stateCache[cacheKey] = currMask.Sum( c => c switch
                {
                    '.' when state.currentRun == 0
                        => consume(new State(state.remainingMask[1..], 0, state.remainingSpec), stateCache),
                    '.' when state.currentRun == state.remainingSpec.FirstOrDefault()
                        => consume(new State(state.remainingMask[1..], 0, state.remainingSpec.Skip(1).ToList()), stateCache),
                    '.' => 0,
                    '#' => consume(new State(state.remainingMask[1..], state.currentRun + 1, state.remainingSpec), stateCache)
                });

            return stateCache[cacheKey];
        }

        //max length of mask ~100 and spec ~ 30
        //max length of run is same as mask
        //so max number of cache entries is 100^2 * 30 = 300,000
        private static string stateToCacheKey(State state) 
            => $"{state.remainingMask.Length}:{state.currentRun}:{state.remainingSpec.Count}";
        record struct State(string remainingMask, int currentRun, List<int> remainingSpec);
    }
}
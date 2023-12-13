using System.Numerics;

namespace _2023_12
{
    internal class Program
    {
        static Dictionary<string, long> stateCache = new();
        static void Main(string[] args)
        {
            var puzzles = File.ReadAllLines("test.txt").Select(line => line.Split(" "))
                .Select(sp => new { masks = sp[0], specs = sp[1].Split(",").Select(int.Parse).ToList() }).ToList();

            Func<long> solve = () => puzzles.Select(a => consume(new State($"{a.masks}.", 0, a.specs))).Sum();
            Console.WriteLine($"Part1: {solve()}");

            puzzles = puzzles.Select(puzzle => new {
                masks = String.Join("?", Enumerable.Repeat(puzzle.masks, 5)),
                specs = Enumerable.Repeat(0, 5).SelectMany(_ => puzzle.specs).ToList()
            }).ToList();

            Console.WriteLine($"Part2: {solve()}");
        }

        static long consume(State state)
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
                        => consume(new State(state.remainingMask[1..], state.currentRun, state.remainingSpec)),
                    '.' when state.currentRun == state.remainingSpec.FirstOrDefault()
                        => consume(new State(state.remainingMask[1..], 0, state.remainingSpec.Skip(1).ToList())),
                    '.' => 0,
                    '#' => consume(new State(state.remainingMask[1..], state.currentRun + 1, state.remainingSpec))
                });

            return stateCache[cacheKey];
        }

        private static string stateToCacheKey(State state) 
            => $"{state.remainingMask}:{state.currentRun}:{String.Join(",", state.remainingSpec)}";
        record struct State(string remainingMask, int currentRun, List<int> remainingSpec);
    }
}
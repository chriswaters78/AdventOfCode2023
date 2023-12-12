using System.Numerics;

namespace _2023_12
{
    internal class Program
    {
        static Dictionary<string, long> stateCache = new Dictionary<string, long>();
        static void Main(string[] args)
        {
            var masks = new List<string>();
            var specs = new List<List<int>>();
            foreach (var line in File.ReadAllLines("test.txt"))
            {
                var sp = line.Split(" ");
                masks.Add(sp[0]);
                specs.Add(sp[1].Split(",").Select(int.Parse).ToList());
            }

            Func<long> solve = () => masks.Zip(specs).Select(a => consume(new State(a.First, 0, a.Second))).Sum();
            long part1 = solve();
            Console.WriteLine($"Part1: {part1}");

            for (int i = 0; i < masks.Count; i++)
            {
                masks[i] = String.Join("?",Enumerable.Repeat(masks[i], 5));
                specs[i] = Enumerable.Repeat(0,5).SelectMany(_ => specs[i]).ToList();
            }
            long part2 = solve();
            Console.WriteLine($"Part2: {part2}");
        }

        static long consume(State state)
        {
            var cacheKey = stateToCacheKey(state);
            if (stateCache.ContainsKey(cacheKey))
            {
                return stateCache[cacheKey];
            }

            if (state.remainingMask.Length == 0)
            {
                if (    (state.remainingSpec.Count == 0 && state.noBroken == 0) 
                    ||  (state.remainingSpec.Count == 1 && state.remainingSpec[0] == state.noBroken))
                {
                    return 1;
                }
                return 0;
            }

            var currMask = state.remainingMask[0] == '?'
                ? new char[] { '.', '#' }
                : new char[] { state.remainingMask[0] };

            long total = 0;
            foreach (var mask in currMask)
            {
                switch (mask)
                {
                    case '.':
                        if (state.noBroken == 0)
                        {
                            total += consume(new State(state.remainingMask[1..], state.noBroken, state.remainingSpec));
                        }
                        else if (state.remainingSpec.Any() && state.noBroken == state.remainingSpec.First())
                        {
                            total += consume(new State(state.remainingMask[1..], 0, state.remainingSpec.Skip(1).ToList()));
                        }
                        break;
                    case '#':
                        if (state.remainingSpec.Any() && state.noBroken >= state.remainingSpec.First())
                        {
                            total += 0;
                        }
                        else
                        {
                            total += consume(new State(state.remainingMask[1..], state.noBroken + 1, state.remainingSpec));
                        }
                        break;
                }
            }

            stateCache[cacheKey] = total;
            return total;
        }

        private static string stateToCacheKey(State state)
        {
            return $"{state.remainingMask}:{state.noBroken}:{String.Join(",", state.remainingSpec)}";
        }
        record struct State(string remainingMask, int noBroken, List<int> remainingSpec);
    }
}
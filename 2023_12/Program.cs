namespace _2023_12
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var masks = new List<string>();
            var specs = new List<List<int>>();
            foreach (var line in File.ReadAllLines("input.txt"))
            {
                var sp = line.Split(" ");
                masks.Add(sp[0]);
                specs.Add(sp[1].Split(",").Select(int.Parse).ToList());
            }

            int part1 = 0;
            for (int i = 0; i < masks.Count; i++)
            {
                var initialState = new State("", masks[i], 0, specs[i]);
                part1 += consume(initialState);
            }
            Console.WriteLine($"Part1: {part1}");

            var masks2 = masks.ToList();
            var specs2 = specs.Select(list => list.ToList()).ToList();
            for (int count = 0; count < 4; count++)
            {
                for (int i = 0; i < masks2.Count; i++)
                {
                    masks2[i] = $"{masks2[i]}{masks[i]}";
                    specs2[i].AddRange(specs[i]);
                }
            }
            int part2 = 0;
            for (int i = 0; i < masks2.Count; i++)
            {
                var initialState = new State("", masks2[i], 0, specs2[i]);
                part2 += consume(initialState);
            }
            Console.WriteLine($"Part2: {part2}");

        }

        static int consume(State state)
        {
            if (state.remainingMask.Length == 0)
            {
                if ((state.remainingSpec.Count == 0 && state.noBroken == 0) || (state.remainingSpec.Count == 1 && state.remainingSpec[0] == state.noBroken))
                {
                    return 1;
                }
                return 0;
            }

            var currMask = state.remainingMask[0] == '?'
                ? new char[] { '.', '#' }
                : new char[] { state.remainingMask[0] };

            int total = 0;
            foreach (var mask in currMask)
            {
                switch (mask)
                {
                    case '.':
                        if (state.noBroken == 0)
                        {
                            total += consume(new State($"{state.consumed}.", state.remainingMask[1..], state.noBroken, state.remainingSpec));
                        }
                        else if (state.remainingSpec.Any() && state.noBroken == state.remainingSpec.First())
                        {
                            total += consume(new State($"{state.consumed}.", state.remainingMask[1..], 0, state.remainingSpec.Skip(1).ToList()));
                        }
                        break;
                    case '#':
                        if (state.remainingSpec.Any() && state.noBroken >= state.remainingSpec.First())
                        {
                            total += 0;
                        }
                        else
                        {
                            total += consume(new State($"{state.consumed}#", state.remainingMask[1..], state.noBroken + 1, state.remainingSpec));
                        }
                        break;
                }
            }

            return total;
        }

        record struct State(string consumed, string remainingMask, int noBroken, List<int> remainingSpec);
    }
}
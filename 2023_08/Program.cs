using System.Numerics;

namespace _2023_08
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var file = File.ReadAllLines("input.txt");
            var instructions = file[0];
            var map = file.Skip(2).ToDictionary(line => line.Split(" = ")[0], line => line.Split(" = ")[1].Trim('(', ')').Split(", ").ToList());

            //{
            //    var current = "AAA";
            //    int steps = 0;
            //    for (var i = 0; i < int.MaxValue; i++)
            //    {
            //        steps++;
            //        current = map[current][instructions[i % instructions.Length] == 'L' ? 0 : 1];
            //        if (current == "ZZZ")
            //        {
            //            break;
            //        }
            //    }
            //    Console.WriteLine($"Part1: {steps}");
            //}

            //do some maths!
            //code below finds every endpoint repeats with a given interval
            //and all the repeats are divisible by 293
            //so we just need to find the lowest common multiple of all the cycles
            //which is a pretty big number

            //can use BigInteger.GreatestCommonDivisor
            //and the relationship LCM = a x b / GCD(a,b)

            long l = 71958382637 * 293;
            Console.WriteLine($"{l}");
            {
                var currents = map.Keys.Where(key => key.EndsWith('A')).ToList();
                var foundEnd = new HashSet<string>();
                Console.WriteLine($"{currents.Count} starting nodes");
                var steps = 0;
                for (var i = 0; i < int.MaxValue; i++)
                {
                    steps++;
                    var nextCurrents = new List<string>();
                    foreach (var current in currents)
                    {
                        var next = map[current][instructions[i % instructions.Length] == 'L' ? 0 : 1];
                        if (next.EndsWith('Z'))
                        {
                            Console.WriteLine($"Found end {next} in {steps}: factor {steps / 293}");
                            foundEnd.Add(next);
                        }
                        nextCurrents.Add(next);
                    }
                    if (nextCurrents.All(key => key.EndsWith('Z')))
                    {
                        break;
                    }
                    currents = nextCurrents;
                }
                Console.WriteLine($"Part2: {steps}");
            }

        }
    }
}
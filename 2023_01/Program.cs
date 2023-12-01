namespace _2023_01
{
    internal class Program
    {
        static string[] numbers = new [] {"one", "two", "three", "four", "five", "six", "seven", "eight", "nine"};
        static void Main(string[] args)
        {
            var file = File.ReadAllLines("input.txt");
            var part1 = file.Select(line => line.Where(c => Char.IsDigit(c)).ToList())
                .Select(arr => int.Parse(arr[0].ToString()) * 10 + int.Parse(arr[arr.Count - 1].ToString()))
                .Sum();

            Console.WriteLine($"Part1: {part1}");

            int part2 = 0;
            foreach (var line in file)
            {
                int first = 0;
                int last = 0;
                for (int x = 0; x < line.Length; x++)
                {
                    if (tryParse(line[x..], out first))
                    {
                        break;
                    }
                }
                for (int x = line.Length - 1; x >=0 ; x--)
                {
                    if (tryParse(line[x..], out last))
                    {
                        break;
                    }
                }

                part2 += first * 10 + last;
            }

            Console.WriteLine($"Part2: {part2}");
        }

        private static bool tryParse(string line, out int val)
        {
            if (Char.IsDigit(line[0]))
            {
                val = int.Parse(line[0].ToString());
                return true;
            }

            foreach (var number in numbers.Select((str,i) => (str,i)))
            {
                if (line.StartsWith(number.str))
                {
                    val = number.i + 1;
                    return true;
                }
            }

            val = 0;
            return false;
        }
    }
}
using System.Numerics;

namespace _2023_24
{
    internal class Program
    {
        
        static void Main(string[] args)
        {
            //19, 13, 30 @ -2,  1, -2

            BigInteger min = 200000000000000L;
            BigInteger max = 400000000000000L;
            //BigInteger min = 7L;
            //BigInteger max = 27L;
            var points = File.ReadAllLines("input.txt").Select(line =>
            {
                var sp = line.Split(" @ ");
                var sp1 = sp[0].Split(", ").Select(BigInteger.Parse);
                var sp2 = sp[1].Split(", ").Select(BigInteger.Parse);

                var points2 = sp1.Zip(sp2).Select(tp => tp.First + tp.Second).ToArray();

                return (sp1.ToArray(), points2.ToArray());
            }).ToArray();

            long valid = 0;
            long outside = 0;
            long parallel = 0;
            long inpast = 0;
            for (int i1 = 0; i1 < points.Length; i1++)
            {
                for (int i2 = i1 + 1; i2 < points.Length; i2++)
                {
                    (var x1, var y1, var z1) = (points[i1].Item1[0], points[i1].Item1[1], points[i1].Item1[2]);
                    (var x2, var y2, var z2) = (points[i1].Item2[0], points[i1].Item2[1], points[i1].Item2[2]);
                    (var x3, var y3, var z3) = (points[i2].Item1[0], points[i2].Item1[1], points[i2].Item1[2]);
                    (var x4, var y4, var z4) = (points[i2].Item2[0], points[i2].Item2[1], points[i2].Item2[2]);
                    
                    var pxn = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
                    var pxd = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                    var pyn = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
                    var pyd = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                    if (pxd == 0 || pyd == 0)
                    {
                        Console.WriteLine("Parallel");
                        Console.WriteLine($"{x1},{y1},{z1} -> {x2},{y2},{z2}");
                        Console.WriteLine($"{x3},{y3},{z3} -> {x4},{y4},{z4}");
                        parallel++;
                        continue;
                    }
                    else
                    {
                        double actualx = (double)pxn / (double) pxd;
                        double actualy = (double)pyn / (double) pyd;

                        if (pxn < 0 && pxd < 0)
                        {
                            pxn *= -1;
                            pxd *= -1;
                        }
                        if (pyn < 0 && pyd < 0)
                        {
                            pyn *= -1;
                            pyd *= -1;
                        }

                        if (pxn >= min * pxd && pxn <= max * pxd && pyn >= min * pyd && pyn <= max * pyd)
                        {
                            if (pxn < x1 * pxd && x1 < x2
                                || pxn > x1 * pxd && x1 > x2
                                || pxn < x3 * pxd && x3 < x4
                                || pxn > x3 * pxd && x3 > x4)
                            {
                                //in past for at least one hailstone
                                inpast++;
                                continue;
                            }
                            else
                            {
                                //Console.WriteLine($"Found intersection at {actualx}, {actualy}");
                                valid++;
                            }
                        }
                        else
                        {
                            outside++;
                            Console.WriteLine($"Found intersection outside area at {actualx}, {actualy}");
                        }
                    }

                }
            }

            //14620 too low
            //14625 includes all parallel, too low!
            //18590 includes all crossings in past, too low?
            Console.WriteLine($"Valid: {valid}, outside {outside}, inpast {inpast}, parallel {parallel}, total {valid + outside + parallel + inpast}");
        }
    }
}

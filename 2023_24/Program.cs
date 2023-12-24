using System.Numerics;

BigInteger min = 200000000000000L;
BigInteger max = 400000000000000L;
//BigInteger min = 7L;
//BigInteger max = 27L;
var points = File.ReadAllLines("input.txt").Select(line =>
{
    var sp = line.Split(" @ ");
    var sp1 = sp[0].Split(", ").Select(long.Parse);
    var sp2 = sp[1].Split(", ").Select(long.Parse);

    return (sp1.ToArray(), sp2.ToArray());
}).ToArray();

(var minx, var miny, var minz) = (points.Min(p => p.Item1[0]), points.Min(p => p.Item1[1]), points.Min(p => p.Item1[2]));
(var minvx, var minvy, var minvz) = (points.Min(p => p.Item2[0]), points.Min(p => p.Item2[1]), points.Min(p => p.Item2[2]));

//there are two points that are always at the same x co-ordinate
//they don't intercept themselves
//334948624416533, 351831296246019, 160929184806206 @ -86, 33, 124
//334948624416533, 296570749940656, 321341704944761 @ -86, 8, -71

//at some time t when it is intercepted by our ray
//its y and z coords must match which is impossible as they don't intercept

//only other option is the ray we cast must have the same x parameters (starting point and slope)
//so it matches the x coordinate at multiple time points

//we can solve for t directly from these
//296570749940656 + 8t = 351831296246019 + 33t
//321341704944761 - 71t = 160929184806206 + 124t
//we start at x = 334948624416533, vx = -86

//part1();
long x = 334948624416533L;
long vx = -86;
var times = new List<(double time, (long x, long y, long z) p, (long vx, long vy, long vz) v)>();
foreach (var point in points)
{
    double t = (double) (point.Item1[0] - x) / ((double) vx - point.Item2[0]);
    if (double.IsNaN(t))
    {
        t = 0;
    }
    times.Add(((long) t, (point.Item1[0], point.Item1[1], point.Item1[2]), (point.Item2[0], point.Item2[1], point.Item2[2])));
    if (!double.IsInteger(t))
    {
        throw new();
    }
}

var vy = (times[1].p.y + times[1].v.vy * times[1].time  - (times[0].p.y + times[0].v.vy * times[0].time)) / (times[1].time - times[0].time);
var y = times[0].p.y + times[0].v.vy * times[0].time - times[0].time * vy;

var vz = (times[1].p.z + times[1].v.vz * times[1].time - (times[0].p.z + times[0].v.vz * times[0].time)) / (times[1].time - times[0].time);
var z = times[0].p.z + times[0].v.vz * times[0].time - times[0].time * vz;

Console.WriteLine($"Part2: {x},{y},{z} @ {vx},{vy},{vz} = {x + y + z}");

//double t1 = (296570749940656L - 351831296246019L) / 25;
//double t2 = (321341704944761L - 160929184806206L) / (124+71);
//double z1 = 160929184806206 + 124 * t1;
//double z2 = 321341704944761 - 71 * t1;

//points = points.OrderBy(p => p.Item1[0]).ToArray();
//for (int i = 1; i < points.Length; i++)
//{
//    Console.WriteLine($"X: {points[i - 1].Item1[0]} to {points[i].Item1[0]}, delta {points[i].Item1[0] - points[i - 1].Item1[0]}");
//}
//points = points.OrderBy(p => p.Item1[1]).ToArray();
//for (int i = 1; i < points.Length; i++)
//{
//    Console.WriteLine($"Y: {points[i - 1].Item1[1]} to {points[i].Item1[1]}, delta {points[i].Item1[1] - points[i - 1].Item1[1]}");
//}
//points = points.OrderBy(p => p.Item1[2]).ToArray();
//for (int i = 1; i < points.Length; i++)
//{
//    Console.WriteLine($"Z: {points[i - 1].Item1[2]} to {points[i].Item1[2]}, delta {points[i].Item1[2] - points[i - 1].Item1[2]}");
//}


//long count = 0;
//for (long i = (long)points[0].Item1[0]; i < max; i += (long)points[0].Item2[0])
//{
//    if (count % 1000000 == 0)
//    {
//        Console.WriteLine($"Checked {i}");
//    }
//    count++;
//}


List<long> primes(long number)
{
    var primes = new List<long>();

    for (int div = 2; div <= Math.Sqrt(number); div++)
        while (number % div == 0)
        {
            primes.Add(div);
            number = number / div;
        }

    if (number > 1)
        primes.Add(number);

    return primes;
}
//part1();

//for (BigInteger t1 = long.MinValue; t1 < long.MaxValue; t1 += long.MaxValue / 100)
//{
//    //assume we have intersected point 1 at t1
//    //we need to find another t at which we intersect point 2
//    for (BigInteger t2 = long.MinValue; t2 < long.MaxValue; t2 += long.MaxValue / 100)
//    {
//        if (t1 == t2)
//            continue;

//        //sum all errors for all lines
//        Console.WriteLine($"t1,t2 = {t1},{t2}, error {sumErrors(t1, t2)}");
//    }
//}
double sumErrors(BigInteger t1, BigInteger t2)
{
    var x0 = points[0].Item1[0] + t1 * points[0].Item2[0];
    var x1 = points[1].Item1[0] + t2 * points[1].Item2[0];
    BigInteger vx = (x1 - x0) / (t2 - t1);

    double totalError = 0;
    foreach (var point in points)
    {
        double tn = (double)point.Item1[0] / (double) (vx - point.Item2[0]);
        //we intercept x axis for this point at tn
        //double yerror = ((double)point.Item1[1] + tn * (double)point.Item2[1]) - tn * (double)vy;
        //totalError += yerror * yerror;
        //double zerror = ((double)point.Item1[2] + tn * (double)point.Item2[2]) - tn * (double)vz;
        //totalError += zerror * zerror;
    }

    return totalError;
}

void part1()
{
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
            if (points[i1].Item1[0] == 334948624416533L && points[i2].Item1[0] == 334948624416533L)
            {
            }
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
                double actualx = (double)pxn / (double)pxd;
                double actualy = (double)pyn / (double)pyd;

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
    Console.WriteLine($"Valid: {valid}, outside {outside}, inpast {inpast}, parallel {parallel}, total {valid + outside + parallel + inpast}");
}



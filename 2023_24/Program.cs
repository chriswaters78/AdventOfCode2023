﻿using System.Numerics;

long min = 200000000000000L;
long max = 400000000000000L;
var points = File.ReadAllLines("input.txt").Select(line =>
{
    var sp = line.Split(" @ ");
    var sp1 = sp[0].Split(", ").Select(long.Parse);
    var sp2 = sp[1].Split(", ").Select(long.Parse);

    return (sp1.ToArray(), sp2.ToArray());
}).ToArray();

(var minx, var miny, var minz) = (points.Min(p => p.Item1[0]), points.Min(p => p.Item1[1]), points.Min(p => p.Item1[2]));
(var minvx, var minvy, var minvz) = (points.Min(p => p.Item2[0]), points.Min(p => p.Item2[1]), points.Min(p => p.Item2[2]));

part1();

//there are two hailstones that are always at the same x co-ordinate
//and they don't intercept
//334948624416533, 351831296246019, 160929184806206 @ -86, 33, 124
//334948624416533, 296570749940656, 321341704944761 @ -86, 8, -71

//at some time t when it is intercepted by our ray
//its y and z coords must match which is impossible as they don't intercept

//only other option is the ray we cast must have the same x parameters (starting point and slope)
//so it matches the x coordinate at multiple time points

//therefore by inspection we start
//at x = 334948624416533, vx = -86
//and it so happens that this is t=0 as well :)
//we can calculate all other points from there

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
                }
            }

        }
    }

    Console.WriteLine($"Part1: {valid}");
}



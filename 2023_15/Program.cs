//rn=1,cm-,qp=3,cm=2,qp-,pc=4,ot=9,ab=5,pc-,pc=6,ot=7

var steps = File.ReadAllText("input.txt").Split(",").ToArray();

int part1 = 0;
foreach (var step in steps)
{    
    part1 += hash(step);
}

Console.WriteLine($"Part1: {part1}");

var boxes = Enumerable.Range(0,255).Select(_ => new List<(string label, int focus)>()).ToArray();
foreach (var step in steps)
{
    var sp = step.Split("=-".ToArray(), StringSplitOptions.RemoveEmptyEntries);
    switch (sp)
    {
        case [var label, var focalLength]:
            {
                var fint = focalLength[0] - '0';
                var box = hash(label);
                bool found = false;
                for (int i = 0; i < boxes[box].Count; i++)
                {
                    if (boxes[box][i].label == label)
                    {
                        boxes[box][i] = (boxes[box][i].label, fint);
                        goto done;
                    }
                }
                if (!found)
                    boxes[box].Add((label, fint));
                done:;
            }
            break;
        case [var label]:
            {
                var box = hash(label);
                boxes[box] = boxes[box].Where(lens => lens.label != label).ToList();
            }
            break;
    }
}

long part2 = 0;
for (int b = 0; b < boxes.Length; b++)
{
    for (int i = 0; i < boxes[b].Count; i++)
    {
        part2 += (1 + b) * (i + 1) * boxes[b][i].focus;
    }
}

Console.WriteLine($"Part2: {part2}");

int hash(string step)
{
    int currentValue = 0;
    foreach (var ch in step)
    {
        currentValue += (int)ch;
        currentValue *= 17;
        currentValue = currentValue % 256;
    }

    return currentValue;
}

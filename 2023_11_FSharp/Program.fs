let EXPANSION = 1000000L;
let file = System.IO.File.ReadAllLines("input.txt");
let watch = System.Diagnostics.Stopwatch();
watch.Start();
  
let answer = Seq.sum <| seq { 
    let points =  [ for y in 0..file.Length-1 do 
                        for x in 0..file[0].Length-1 do 
                            if file[y][x] = '#' then yield (x,y) ]
    
    let fullRows = Set.ofList <| List.map snd points 
    let fullColumns = Set.ofList <| List.map fst points 
    
    for (px1,py1) in points do
        for (px2,py2) in points do
        let (x1,x2) = (min px1 px2, max px1 px2)
        let (y1,y2) = (min py1 py2, max py1 py2)
        let s1 = Seq.sum <| seq { for x in [x1+1..x2] do yield if fullColumns.Contains(x) then 1L else EXPANSION }
        let s2 = Seq.sum <| seq { for y in [y1+1..y2] do yield if fullRows.Contains(y) then 1L else EXPANSION }
        yield s1 + s2 }

printfn $"Distance {answer >>> 1} for expansion {EXPANSION} in {watch.ElapsedMilliseconds}ms";

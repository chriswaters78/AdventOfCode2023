let EXPANSION = 1000000L;
let file = System.IO.File.ReadAllLines("input.txt");

let points =  [ for y in 0..file.Length-1 do 
                    for x in 0..file[0].Length-1 do 
                        if file[y][x] = '#' then yield (x,y) ]

let fullRows = seq { for (_,y) in points do yield y } |> Set.ofSeq
let fullColumns = seq { for (x,_) in points do yield x } |> Set.ofSeq
  
let answer = Seq.sum <| seq { for p1 in 0..points.Length-1 do
                                for p2 in p1+1..points.Length-1 do
                                let (px1,py1) = points[p1]
                                let (px2,py2) = points[p2]
                                let (x1,x2) = (min px1 px2, max px1 px2)
                                let (y1,y2) = (min py1 py2, max py1 py2)
                                let s1 = seq { for x in [x1+1..x2] do yield if fullColumns.Contains(x) then 1L else EXPANSION }
                                let s2 = seq { for y in [y1+1..y2] do yield if fullRows.Contains(y) then 1L else EXPANSION }
                                yield Seq.sum s1 + Seq.sum s2 }
                
printfn $"Distance {answer} for expansion {EXPANSION}";

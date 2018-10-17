namespace Zander
open Zander.Internal
open System.Collections.Generic

type ParsedRow = {
        Name: string
        Cells: MatchCell[]
    }
    with
        member this.Values = this.Cells |> Array.map (fun r->r.Value)
        static member Create (row:MatchRow, name)={Name=name; Cells=row.Cells }
        override self.ToString()= sprintf "%A" self

type ParsedBlock={
        Name: string
        Rows: ParsedRow[]
    }
    with
        override self.ToString()= sprintf "%A" self


/// The usage of ParserBuilder is discouraged, prefer a list of BlockEx and compose thoose
type ParserBuilder(array : (string*BlockEx) list)=
    member this.Block(name: string, expression : string ) = 
        ParserBuilder(array @ [ ( name, BlockEx(expression) )])

    member this.Parse(blocks : string array array) : ParsedBlock seq=
        let matrix = blocks 

        let rec parse index : ParsedBlock list =
            let asCsv (m:string[]) =
                System.String.Join(", ", ( Array.map (fun s->sprintf "\"%s\"" s) m) )

            if index >= Array.length matrix then
                []
            else
                let maybeNext =  array |> List.tryPick (fun (name,block)-> 
                    let m = block.Match( (matrix |> Array.skip index))
                    if m.Success then Some (m, name) else None
                )
                match maybeNext with
                    | Some (next,name) -> 
                        let parsed = next.RowAndNames
                        
                        let nextIndex = index + (List.length parsed)
                        [ { Name=name; 
                            Rows= parsed |> List.map ParsedRow.Create |> List.toArray } 
                        ] @ (parse nextIndex) 
                    | None -> 
                        (failwithf "could not find block to interpret %s" (asCsv (Array.item index matrix)))

        parse 0 |> List.toSeq

    new() =
        ParserBuilder([])
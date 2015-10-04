namespace Zander
open Zander.Internal
open System.Collections.Generic

[<System.Obsolete("Use MatchRow")>]
type ParsedRow = {
        Name: string
        Values: KeyValuePair<string,string>[]
    }
    with
        override self.ToString()=
            let kvs = self.Values |> Array.map (fun kv-> sprintf "%s : %s" kv.Key kv.Value)
            sprintf "%s=%s" self.Name (String.concat "," kvs )

[<System.Obsolete("Use MatchBlock")>]
type ParsedBlock={
        Name: string
        Rows: ParsedRow array
    }
    with
        override self.ToString() = 
            let rows = 
                self.Rows
                |> Seq.map ( fun row-> row.ToString()) 
                |> String.concat ",\n" 

            sprintf "{%s : %s}" self.Name rows


[<System.Obsolete("Use BlockEx")>]
type BuildingBlock( name:string, block: RecognizesRows list)=
    member this.name = name
    member this.block = block

[<System.Obsolete("Use BlockEx")>]
type ParserBuilder(array : BuildingBlock list)=
    let array = array
    let opts = ParseOptions.Default
    let rowsOf v = 
        let to_kv (v:(string*string)) : KeyValuePair<string,string>=
            new KeyValuePair<string,string>(fst v, snd v)
        let valuesOf v' =
            v'
            |> List.map Parse.Result.value
            |> List.choose Parse.Token.tryKeyValue
            |> List.map to_kv

        v |> List.map (
             fun (row,name)-> (valuesOf row) , name
             )

    member internal this.RawBlock  (x : string* (RecognizesRows list)) = 
        new ParserBuilder(array @ [ new BuildingBlock(fst x, snd x) ])

    member this.Block(name: string, x : string ) = 
        new ParserBuilder(array @ [ new BuildingBlock( name, (Lang.block x) )])

    member this.Parse(blocks : string array array) : ParsedBlock seq=
        let matrix = 
            blocks |> Array.map Array.toList
                   |> Array.toList

        let to_rows (parsed: (Parse.Result list*string) list) =
            parsed |> rowsOf
                   |> List.map (fun next -> { Name= (snd next); Values= (fst next |> List.toArray) } ) 
                   |> List.toArray

        let rec parse index : ParsedBlock list =
            let as_csv (m:string list) =
                System.String.Join(", ", ( List.map (fun s->sprintf "\"%s\"" s) m) )

            if index >= List.length matrix then
                []
            else
                let maybeNext =  array |> List.tryFind (fun sp-> (Match.block (Parse.block (sp.block) opts (matrix |> List.skip index)) ))
                match maybeNext with
                    | Some next -> 
                        let parsed = Parse.block (next.block) opts (matrix|> List.skip index)
                        let nextIndex = index + (List.length parsed)
                        [ { Name=next.name; Rows= (to_rows parsed) } ] @ (parse nextIndex) 
                    | None -> 
                        (failwithf "could not find block to interpret %s" (as_csv (List.item index matrix)))

        parse 0 |> List.toSeq

    new() =
        ParserBuilder([])
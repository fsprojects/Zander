namespace Zander
open Zander.Internal
open System.Collections.Generic

type ParsedRow = {
        Name: string
        Values: KeyValuePair<string,string>[]
    }
    with
        override self.ToString()=
            let kvs = self.Values |> Array.map (fun kv-> sprintf "%s : %s" kv.Key kv.Value)
            sprintf "%s=%s" self.Name (String.concat "," kvs )

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

type Builder(array : (string *( (NumberOf* BlockType list * string ) list)) list)=
    let array = array
    let rowsOf v = 
        let to_kv value : KeyValuePair<string,string>=
            match value with
               | Parse.Value (n,v) -> new KeyValuePair<string,string>(n,v)
               | _ -> failwith "!"
        // todo: fix this!
        let valuesOf v' =
            v'
            |> List.map Parse.Result.value
            |> List.filter Parse.Token.isValue
            |> List.map to_kv

        v |> List.map (
             fun (row,name)-> (valuesOf row) , name
             )

    member this.RawBlock(x : string*( (NumberOf* BlockType list * string) list )) = 
        new Builder(array @ [ x ])

    member this.Block(name: string, x : string ) = 
        new Builder(array @ [ ( name, (Api.interpret x) )])

    member this.Parse(blocks : string array array) : ParsedBlock seq=
        let matrix = 
            blocks |> Array.map Array.toList
                   |> Array.toList

        let to_rows (parsed: (Parse.Result list*string) list) =
            parsed |> rowsOf
                   |> List.map (fun next -> { Name= (snd next); Values= (fst next |> List.toArray) } ) 
                   |> List.toArray

        let rec parse index : ParsedBlock list =
            if index >= List.length matrix then
                []
            else
                let maybeNext =  array |> List.tryFind (fun sp-> (Match.block (snd sp) index matrix ) )
                match maybeNext with
                    | Some next -> 
                        let parsed = Parse.block (snd next) index matrix
                        let nextIndex = index + (List.length parsed)
                        [ { Name=fst next; Rows= (to_rows parsed) } ] @ (parse nextIndex) 
                    | None -> 
                        (failwithf "could not find expression block for index %i" index)

        parse 0 |> List.toSeq

    new() =
        Builder([])
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

type BuildingBlock( name:string, block: BlockRecognizer)=
    member this.name = name
    member this.block = block

type Builder(array : BuildingBlock list)=
    let array = array
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

    member internal this.RawBlock  (x : string* BlockRecognizer) = 
        new Builder(array @ [ new BuildingBlock(fst x, snd x) ])

    member this.Block(name: string, x : string ) = 
        new Builder(array @ [ new BuildingBlock( name, (Api.interpret x) )])

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
                let maybeNext =  array |> List.tryFind (fun sp-> (Match.block (sp.block) index matrix ) )
                match maybeNext with
                    | Some next -> 
                        let parsed = Parse.block (next.block) index matrix
                        let nextIndex = index + (List.length parsed)
                        [ { Name=next.name; Rows= (to_rows parsed) } ] @ (parse nextIndex) 
                    | None -> 
                        (failwithf "could not find expression block for index %i" index)

        parse 0 |> List.toSeq

    new() =
        Builder([])
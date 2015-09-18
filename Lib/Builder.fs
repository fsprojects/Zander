namespace Zander
open Zander.Internal

type ParsedRow = {
        Name: string
        Values: string[]
    }
    with
        override self.ToString()=
                sprintf "%s=%s" self.Name (String.concat "," self.Values )

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

    member this.RawBlock(x : string*( (NumberOf* BlockType list * string) list )) = 
        new Builder(array @ [ x ])

    member this.Block(name: string, x : string ) = 
        new Builder(array @ [ ( name, (Api.interpret x) )])

    member this.Parse(blocks : string array array) : ParsedBlock seq=
        let matrix = 
            blocks |> Array.map Array.toList
                   |> Array.toList

        let to_rows (parsed: (Parse.Result list*string) list) =
            parsed |> Parse.rowsOf
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
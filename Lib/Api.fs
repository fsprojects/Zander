namespace Zander
open Zander.Internal

type Size={Height:int;Width:int}
    with
        override self.ToString()=sprintf "(%i, %i)" self.Height self.Width

type CellType=
    | Unknown=0
    | Constant=1
    | Value=2

type MatchCell(matches: Parse.Result)=
    let result_value = matches |>Parse.Result.tryValue 
    let null_if_none opt=
        match opt with
        | Some v->v
        | None -> null

    member self.Success with get()=Parse.Result.isOk matches
    member self.Name with get()=
                                result_value
                                    |>Option.bind Parse.Token.tryKey
                                    |>null_if_none
                                    
    member self.Value with get()= 
                                result_value
                                    |>Option.bind Parse.Token.tryValue
                                    |>null_if_none
    member self.CellType with get()=
                                    Option.opt{
                                        let! token = result_value
                                        return match token.cell with
                                                | Zander.Internal.CellType.Value _ -> CellType.Value
                                                | _ -> CellType.Constant
                                    } |> (fun v -> match v with | None -> CellType.Unknown; | Some v -> v ; )
    override self.ToString()= sprintf "MatchCell(%O, %s : %s)" self.CellType self.Name self.Value

type MatchRow(matches: Parse.Result list)=
    let cells = matches |> List.map (fun m->new MatchCell(m))
    let to_tuples ()=
            matches 
            |> List.choose Parse.Result.tryValue
            |> List.choose Parse.Token.tryKeyValue
    let to_dictionary ()=
        to_tuples()
            |> dict
    member self.Success with get() = Match.expression matches
    member self.Length with get() = List.length matches
    member self.Cells with get() =  cells |> List.toArray
    member self.ToDictionary() = to_dictionary()
    override self.ToString() = 
        sprintf "MatchRow( %s )" (to_tuples() 
                                    |> Seq.map (fun (k,v) -> sprintf "(%s, %s)" k v) 
                                    |> String.concat "; ")

type MatchBlock(matches: Parse.RecognizedBlock)=
    let height = matches |> List.length
    let width = matches |> List.map (fst >> List.length) |> List.max
    let size = {Height=height;Width=width}
    
    let rows = matches |> List.map (fun (m,name)->(new MatchRow(m),name))

    member self.Success with get() = Match.block matches
    member self.Size with get(): Size= size
    member self.Rows with get()= rows |> List.map fst |> List.toArray
    member self.WithName name = rows 
                                |> List.filter (snd >> (=) name) 
                                |> List.map fst 
                                |> List.toArray

type BlockEx(expression:string, options: ParseOptions)=
    let block=Lang.block expression
    member internal self.Match (input:string array array, position:int option) : MatchBlock=
        let start = match position with |Some v->v;|None -> 0
        let parsed = Parse.block block options (input |> Array.skip start 
                                                      |> Array.map Array.toList
                                                      |> Array.toList)
        MatchBlock(parsed)
    member self.Match (input:string array array, position:int) : MatchBlock=
        self.Match(input, Some position)

    member self.Match (input:string array array) : MatchBlock=
        self.Match(input, None)

    member self.Matches (input:string array array)=
        let rec matchit position=
            let m = self.Match(input, Some position)
            if m.Success then
                m :: matchit (position+m.Size.Height)
            else
                []
        matchit 0 |> List.toArray
    member self.Split ( input: string array array) : string array array array=
        input 
            |> Array.toList 
            |> List.map Array.toList
            |> Matches.split_list (fun ( arr :string list list)->
                let parsed = Parse.block block options arr
                (Match.block parsed, List.length parsed)
            )
        |> List.map (List.map List.toArray)
        |> List.map List.toArray
        |> List.toArray
    new (expression:string)=
        new BlockEx(expression, ParseOptions.Default)


type RowEx(expression:string, options: ParseOptions)=
    let row= Lang.row expression
    member self.Match (input:string array) : MatchRow=
        new MatchRow(Parse.expression row options (input |> Array.toList))
    new (expression:string)=
        new RowEx(expression, ParseOptions.Default)

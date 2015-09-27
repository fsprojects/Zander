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

type MatchRow(matches: Parse.Result list)=
    let cells = matches |> List.map (fun m->new MatchCell(m))
    let to_dictionary ()=
        matches 
            |> List.choose Parse.Result.tryValue
            |> List.choose Parse.Token.tryKeyValue
            |> dict
    member self.Success with get() = Match.expression matches
    member self.Length with get() = List.length matches
    member self.Cells with get() =  cells |> List.toArray
    member self.ToDictionary() = to_dictionary()

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

type BlockEx(expression:string)=
    let block=Lang.block expression
    member self.Match (input:string array array,position:int option) : MatchBlock=
        let start = match position with |Some v->v;|None -> 0
        let parsed = Parse.block block start (input |> Array.map Array.toList
                                                    |> Array.toList)
        MatchBlock(parsed)

    member self.Match (input:string array array) : MatchBlock=
        self.Match(input, None)
    member self.Match (input:string array array, position:int ) : MatchBlock=
        self.Match(input, Some position)

type RowEx(expression:string)=
    let row= Lang.row expression
    member self.Match (input:string array) : MatchRow=
        new MatchRow(Parse.expression row (input |> Array.toList))
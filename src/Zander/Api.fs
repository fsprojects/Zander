namespace Zander
open Zander.Internal

type Size={Height:int;Width:int}
    with
        override self.ToString()=sprintf "(%i, %i)" self.Height self.Width

type CellType=
    | Unknown=0
    | Constant=1
    | Value=2

type MatchCell(matches: Result<_,_>)=
    let resultValue = matches |>Result.tryValue 
    let cellType =resultValue
                  |> Option.map (fun token->
                              match token.cell with
                              | Zander.Internal.CellType.Value _ -> CellType.Value
                              | _ -> CellType.Constant)
                  |> Option.defaultValue CellType.Unknown
    let value = resultValue
               |>Option.bind Token.tryValue
               |>Option.defaultValue (null:string)
    let name = resultValue
               |>Option.bind Token.tryKey
               |>Option.defaultValue (null:string)
    member self.Success with get()=Result.isOk matches
    member self.Name with get()=name
    member self.Value with get()= value                                 
    member self.CellType with get()=cellType
                                    
    override self.ToString()= sprintf "MatchCell(%O, '%s' : '%s')" self.CellType self.Name self.Value
    override self.Equals obj=
        match obj with
        | :? MatchCell as cell->
            self.Success = cell.Success 
            && self.Name = cell.Name 
            && self.Value = cell.Value 
            && self.CellType = cell.CellType
        | _ -> false
    override  self.GetHashCode() = (self.Success, self.Name, self.Value, self.CellType).GetHashCode()
type MatchRow(matches: Result<_,_> list)=
    let cells = matches |> List.map MatchCell
    let toTuples ()=
            matches 
            |> List.choose Result.tryValue
            |> List.choose Token.tryKeyValue
    member self.Success with get() = Row.isMatch matches
    member self.Length with get() = List.length matches
    member self.Cells with get() =  cells |> List.toArray
    member self.ToDictionary() = toTuples() |> dict
    override self.ToString() = 
        sprintf "MatchRow( %s )" (toTuples() 
                                    |> Seq.map (fun (k,v) -> sprintf "(%s, %s)" k v) 
                                    |> String.concat "; ")

type MatchBlock(matches: RecognizedBlock)=
    let height = matches |> List.length
    let width = matches |> List.map (fst >> List.length) |> List.max
    let size = {Height=height;Width=width}
    
    let rows = matches |> List.map (fun (m,name)->(MatchRow m, name))

    member self.Success with get() = Block.isMatch matches
    member self.Size with get(): Size= size
    member self.Rows with get()= rows |> List.map fst |> List.toArray
    member internal self.RowAndNames with get()= rows
    member self.RowNames with get()= rows |> List.map snd |> List.toArray
    member self.WithName name = rows 
                                |> List.filter (snd >> (=) name) 
                                |> List.map fst 
                                |> List.toArray

type BlockEx(expression:string, options: ParseOptions)=
    let block=Block.recognizer expression
    member internal self.Match (input:string array array, position:int option) : MatchBlock=
        let start = match position with |Some v->v;|None -> 0
        let parsed = Block.parse block options (input |> Array.skip start 
                                                      |> Array.map Array.toList
                                                      |> Array.toList)
        MatchBlock(parsed)
    member self.Match (input:string array array, position:int) : MatchBlock=
        self.Match(input, Some position)

    member self.Match (input:string array array) : MatchBlock=
        self.Match(input, None)
    member self.IsMatch (input:string array array, position:int) : bool=
        self.Match(input, Some position).Success
    member self.IsMatch (input:string array array) : bool=
        self.Match(input, None).Success
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
            |> Engine.splitList (fun ( arr :string list list)->
                let parsed = Block.parse block options arr
                (Block.isMatch parsed, List.length parsed)
            )
        |> List.map (List.map List.toArray)
        |> List.map List.toArray
        |> List.toArray
    new (expression:string)=BlockEx(expression, ParseOptions.Default)


type RowEx(expression:string, options: ParseOptions)=
    let row= Row.recognizer expression
    member self.Match (input:string array) : MatchRow=
        new MatchRow(Row.parse row options (input |> Array.toList))
    new (expression:string)=RowEx(expression, ParseOptions.Default)

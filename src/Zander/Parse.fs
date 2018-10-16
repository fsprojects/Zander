namespace Zander.Internal
open Zander
open System
type RecognizesCells = (NumberOf*CellType)

type RecognizesRow = {recognizer:RecognizesCells list;name:string}
    with
        override self.ToString()=sprintf "%s %O" self.name self.recognizer
type RecognizesRows = NumberOf*RecognizesRow

module Parse=

    type Token={ value:string; cell:CellType }
        with
            override self.ToString()=
                match self.cell with
                    | Empty -> "Empty"
                    | Const c -> sprintf "'%s'" c
                    | Value v -> sprintf "%O = %s" (Value v) self.value
                    | Or cs -> sprintf "%O = %s" (Or cs) self.value
            static member tryValue (t:Token)=
                match t.cell with
                    | Value (name) -> Some (t.value)
                    | _ -> None
            static member tryKeyValue (t:Token)=
                match t.cell with
                    | Value (name) -> Some (name, t.value)
                    | _ -> None
            static member tryKey (t:Token)=
                match t.cell with
                    | Value (name) -> Some name
                    | _ -> None

    type Result=
        | Ok of Token
        | WrongConstant of (string * string)
        | UnRecognized of string
        | Missing 
        with
            static member tryValue result=
                match result with
                    | Ok v -> Some v
                    | _ -> None

            static member isOk result=
                match result with
                    | Ok v -> true
                    | _ -> false

            static member value result=
                Option.get (Result.tryValue result)

    type RecognizedBlock=((Result list*string) list)
    type ColumnsAndPosition = InputAndPosition<string list>
    type ResultAndLength = (Result*int)
    open Zander.Internal.Matches


    [<CompiledName("Expression")>]
    let expression (expr:(NumberOf*CellType) list) (opts: ParseOptions) row : Result list=
        let valueMatchEmpty = opts.HasFlag(ParseOptions.ValueMatchesEmpty)
        let rec columnMatch (columnExpr:CellType) column=
            let value = { value=column;cell= columnExpr }

            match columnExpr, column with
                | Empty, "" -> Ok( value )
                | Const v1,v2 when v1=v2-> 
                    Ok( value )
                | Const v1,v2 when v1<>v2-> 
                    WrongConstant (v1, v2)
                | Value n, v when String.IsNullOrEmpty(v) && valueMatchEmpty -> 
                    Ok( value )
                | Value n, v when not(String.IsNullOrEmpty(v)) ->
                    Ok( value )
                | Value n, v ->
                    UnRecognized v
                | Or cs,v->
                    let rec first v = function
                        | [] -> UnRecognized v
                        | head:: tail-> 
                            let a' = columnMatch head v;
                            if a' |> Result.isOk then
                                a'
                            else
                                first v tail
                    first v cs
                | _, v-> UnRecognized v

        let toResult matchResult = 
            match matchResult with
                | MatchOk v -> v
                | MatchEmptyList -> Missing
                | MatcherMissing v -> UnRecognized v
                | MatchFailure -> failwith "match failure"

        matches columnMatch Result.isOk expr row |> List.map toResult

    [<CompiledName("Block")>]
    let block (expr:(NumberOf*RecognizesRow) list) (opts: ParseOptions) blocks : RecognizedBlock=
        let blockMatch (r:RecognizesRow) (row:string list)=
            let result = expression r.recognizer opts row
            (result,r.name)
        let blockIsOk b =
                b   |> fst
                    |> List.forall Result.isOk

        let toResult matchResult: Result list*string=
            match matchResult with
                | MatchOk v -> v
                | MatchEmptyList -> [Missing],"Match empty list"
                | MatcherMissing v ->
                    if opts.HasFlag(ParseOptions.BlockMatchesAll) then
                        v|> List.map UnRecognized,"Matcher missing"
                    else
                        [], "Matcher missing"
                | MatchFailure -> failwith "Match failure"

        let result = matches blockMatch blockIsOk expr blocks
        let isMatcherMissing m=
             match m with
             | MatcherMissing _ -> true
             | _ -> false

        if opts.HasFlag(ParseOptions.BlockMatchesAll) then result 
        else result |> List.filter (not<< isMatcherMissing)
        |> List.map toResult

    /// Returns the values of 'Value' where the match is ok
    [<CompiledName("RowsOf")>]
    let rowsOf v = 
        let valuesOf v' =
            v'
            |> List.choose Result.tryValue
            |> List.choose Token.tryValue

        v |> List.map (fun (row,name)-> (valuesOf row) , name)


namespace Zander.Internal
open Zander
open System
type RecognizesCells = (NumberOf*CellType)

type RecognizesRow = {recognizer:RecognizesCells list;name:string}
    with
        override self.ToString()=sprintf "%s %O" self.name self.recognizer
type RecognizesRows = NumberOf*RecognizesRow

type Token={ value:string; cell:CellType }
    with
        override self.ToString()=
            match self.cell with
                | Empty -> "Empty"
                | Const c -> sprintf "'%s'" c
                | Value v -> sprintf "%O = %s" (Value v) self.value
                | Or cs -> sprintf "%O = %s" (Or cs) self.value
module Token=
    let createValue (name, value)={ value=value; cell=CellType.Value name }
    let createConstant (name, value)={ value=value; cell=CellType.Const name }
    let tryValue (t:Token)=
        match t.cell with
            | Value (name) -> Some (t.value)
            | _ -> None
    let tryKeyValue (t:Token)=
        match t.cell with
            | Value (name) -> Some (name, t.value)
            | _ -> None
    let tryKey (t:Token)=
        match t.cell with
            | Value (name) -> Some name
            | _ -> None
type Error = 
    | WrongConstant of (string * string)
    | UnRecognized of string
    | Missing
module Result=
    let tryValue result=
        match result with
            | Ok v -> Some v
            | _ -> None

    let isOk result=
        match result with
            | Ok v -> true
            | _ -> false

    let value result=
        Option.get (tryValue result)

type RecognizedBlock=((Result<Token,Error> list*string) list)
type ColumnsAndPosition = InputAndPosition<string list>
type ResultAndLength = (Result<Token,Error>*int)
open Zander.Internal.Matches

module Row=

    [<CompiledName("Parse")>]
    let parse (expr:(NumberOf*CellType) list) (opts: ParseOptions) row : Result<Token,Error> list=
        let valueMatchEmpty = opts.HasFlag(ParseOptions.ValueMatchesEmpty)
        let rec columnMatch (columnExpr:CellType) column=
            let value = { value=column;cell= columnExpr }

            match columnExpr, column with
                | Empty, "" -> Ok( value )
                | Const v1,v2 when v1=v2-> 
                    Ok value
                | Const v1,v2 when v1<>v2-> 
                    Error <| WrongConstant (v1, v2)
                | Value n, v when String.IsNullOrEmpty(v) && valueMatchEmpty -> 
                    Ok value
                | Value n, v when not(String.IsNullOrEmpty(v)) ->
                    Ok value
                | Value n, v ->
                    Error <| UnRecognized v
                | Or cs,v->
                    let rec first v = function
                        | [] -> Error <| UnRecognized v
                        | head:: tail-> 
                            let a' = columnMatch head v;
                            if a' |> Result.isOk then
                                a'
                            else
                                first v tail
                    first v cs
                | _, v-> Error <| UnRecognized v

        let mapToError = function
                | MatchEmptyList -> Missing
                | (MatcherMissing v) -> UnRecognized v
                | MatchFailure -> failwith "match failure"

        let res= matches columnMatch Result.isOk expr row
        let foldError =Result.mapError mapToError >> Result.bind id
        res |> List.map foldError


module Block=

    [<CompiledName("Parse")>]
    let parse (expr:(NumberOf*RecognizesRow) list) (opts: ParseOptions) blocks : RecognizedBlock=
        let blockMatch (r:RecognizesRow) (row:string list)=
            let result = Row.parse r.recognizer opts row
            (result,r.name)
        let blockIsOk b =
                b   |> fst
                    |> List.forall Result.isOk

        let toResult matchResult: (Result<_,_> list)*string=
            match matchResult with
                | Ok v -> v
                | Error MatchEmptyList -> [Error Missing],"Match empty list"
                | Error (MatcherMissing v) ->
                    if opts.HasFlag(ParseOptions.BlockMatchesAll) then
                        v|> List.map (Error << UnRecognized),"Matcher missing"
                    else
                        [], "Matcher missing"
                | Error MatchFailure -> failwith "Match failure"

        let result = matches blockMatch blockIsOk expr blocks
        let isMatcherMissing m=
             match m with
             | Error (MatcherMissing _) -> true
             | _ -> false
        if opts.HasFlag(ParseOptions.BlockMatchesAll) then result 
        else result |> List.filter (not<< isMatcherMissing)
        |> List.map toResult



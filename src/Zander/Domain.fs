namespace Zander.Internal
open System
open System.Text.RegularExpressions

open Zander
open Zander.Internal.Engine
open Zander.Internal.StringAndPosition

/// Type of Cell
type CellType=
    /// empty constant
    | Empty
    /// constant
    | Const of string 
    /// variable with name
    | Value of string 
    | Or of CellType list
    with
        override self.ToString()=
            match self with
                | Empty -> "Empty"
                | Const c -> sprintf "'%s'" c
                | Value v -> sprintf "@%s" v
                | Or cs-> cs |> List.map string |> String.concat "||" |> sprintf "(%s)"
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
type BlockParseError = 
    | WrongConstant of (string * string)
    | UnRecognized of string
    | Missing
module internal Result=
    let toOption = function | Ok v -> Some v | _ -> None
    let isOk =function | Ok v -> true | _ -> false
    let value = function | Ok v -> v | Error err -> failwithf "%A" err

type RecognizedBlock=((Result<Token,BlockParseError> list*string) list)
type ColumnsAndPosition = InputAndPosition<string list>
type ResultAndLength = (Result<Token,BlockParseError>*int)
[<AutoOpen>]
module internal ParseHelpers=

    let startsWithQuote input = regexMatchI "^\"" input
    let regex = Regex(@"[^""\\]*(?:\\.[^""\\]*)*""")
    let indexOfFirstNonEscapedQuote (input :StringAndPosition)=
        let m = regex.Match(input.input, input.position)
        if m.Success then 
            Some (input.position+ m.Length-1)
        else
            None
    let (|LooksLikeConstant|) (input:StringAndPosition) : (StringAndLength) option=
        let withoutFirstQuote = startsWithQuote input |> Option.map (fun m->(sIncr (snd m) input))
        let constantAndLength i= 
            let constant = input.input.Substring(input.position+1, (i-input.position-1))
            let length = i-input.position+1
            (constant, length)
        withoutFirstQuote
        |> Option.bind indexOfFirstNonEscapedQuote
        |> Option.map constantAndLength

    let rec parseCells parseCell lengthOfInput input =
        let {input = s; position= i} = input
        let head = parseCell input
        let l = i+ (snd head)
        if l >= lengthOfInput input then
            [head]
        else
            head :: parseCells parseCell lengthOfInput {input=s; position=l}

    let rowRegex = Regex(@"
          ^
          (?<columns>[^:]*) \s*
          (
            \: \s* 
            (?<name>\w*) \s* (?<modifier>[+?*]?) \s*
          )?
          $
    ", RegexOptions.IgnorePatternWhitespace)

module Row=
    /// Parse single row expression to row recognizer
    [<CompiledName("Recognizer")>]
    let recognizer (v:string) : RecognizesCells list=
       let rec parseCell = function 
        |{position=0;input=""} -> failwith "Cannot parse empty cell!"
        |v ->
            let unwrap_to_cell (v:string) : (NumberOf*CellType) option =
                let value, _ =parseCell (firstPosition v)
                value
            match v with
                | RegexMatch @"^\s+" ([g], l) -> None, l
                | RegexMatch @"^\|" ([g], l) -> None, l
                | RegexMatch @"^\(([^)]*)\)" ([_;s], l) ->
                    let cells = parseCells parseCell length (firstPosition s.Value)
                                |> List.choose fst
                    let conditions = 
                            cells |> List.map snd
                    Some (One, (Or conditions)), l
                | RegexMatch "^(_)([+*?])?" ([_;_;number], l) -> 
                    Some (NumberOf.parse number.Value, Empty), l
                | LooksLikeConstant (Some (c, l)) -> 
                    Some((One,Const(c))), l 
                | RegexMatch @"^\@(\w+)([+*?])?" ([_;value;number], l) -> 
                    Some( (NumberOf.parse number.Value, Value( value.Value ))) , l
                | RegexMatch @"^(\w+)([+*?])?" ([_;value;number], l) -> 
                    Some( (NumberOf.parse number.Value, Const( value.Value ))) , l
                | _ -> 
                    failwithf "Could not interpret: '%s' at position %i" (sub v) (position v)

       parseCells parseCell length {input =v; position=0}
            |> List.choose fst
    /// parse a row using a row recognizer
    [<CompiledName("Parse")>]
    let parse (expr:RecognizesCells list) (opts: ParseOptions) row : Result<Token,BlockParseError> list=
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

    [<CompiledName("IsMatch")>]
    let isMatch recognized = recognized |> List.forall Result.isOk

module Rows=

    /// Parse row expression to row recognizer
    [<CompiledName("Recognizer")>]
    let recognizer (v:string) : RecognizesRows=
        let m = rowRegex.Match(v)
        let columns = Row.recognizer (m.Groups.["columns"].Value)
        let name =  m.Groups.["name"].Value
        let modifier = NumberOf.parse m.Groups.["modifier"].Value

        (modifier,{recognizer= columns; name= name})

module Block=
    [<CompiledName("Recognizer")>]
    let recognizer (s : string) : (RecognizesRows list)=
        let rows = s.Split([| '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.filter ( not << String.IsNullOrWhiteSpace)
                        |> Array.map Rows.recognizer 
        Array.toList rows

    [<CompiledName("Parse")>]
    let parse (expr:RecognizesRows list) (opts: ParseOptions) blocks : RecognizedBlock=
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

    [<CompiledName("IsMatch")>]
    let isMatch (recognized:RecognizedBlock) =
        recognized
        |> List.map fst
        |> List.collect id
        |> List.forall Result.isOk


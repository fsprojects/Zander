namespace Zander.Internal
open System
open System.Text.RegularExpressions

module Lang=

    open Zander.Internal.StringAndPosition

    module private Helpers=
        let startsWithQuote input = regexMatchI "^\"" input
        let regex = Regex(@"[^""\\]*(?:\\.[^""\\]*)*""")
        let indexOfFirstNonEscapedQuote (input :StringAndPosition)=
            let m = regex.Match(input.input, input.position)
            if m.Success then 
                Some (input.position+ m.Length-1)
            else
                None

    open Helpers
    let (|LooksLikeConstant|) (input:StringAndPosition) : (StringAndLength) option=
        let withoutFirstQuote = startsWithQuote input |> Option.map (fun m->(sIncr (snd m) input))
        let constantAndLength i= 
            let constant = input.input.Substring(input.position+1, (i-input.position-1))
            let length = i-input.position+1
            (constant, length)
        withoutFirstQuote
        |> Option.bind indexOfFirstNonEscapedQuote
        |> Option.map constantAndLength
    let numberOf (g:string)=
        match g with
            | "" -> One
            | "+" -> Many
            | "*" -> ZeroOrMany
            | "?" -> ZeroOrOne
            | v -> failwithf "Could not interpret: '%s' as number of" v

    let rec parseCells parseCell lengthOfInput input =
        let {input = s; position= i} = input
        let head = parseCell input
        let l = i+ (snd head)
        if l >= lengthOfInput input then
            [head]
        else
            head :: parseCells parseCell lengthOfInput {input=s; position=l} 

    [<CompiledName("Row")>]
    let row (v:string) : RecognizesCells list=
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
                    Some (numberOf number.Value, Empty), l
                | LooksLikeConstant (Some (c, l)) -> 
                    Some((One,Const(c))), l 
                | RegexMatch @"^\@(\w+)([+*?])?" ([_;value;number], l) -> 
                    Some( (numberOf number.Value, Value( value.Value ))) , l
                | RegexMatch @"^(\w+)([+*?])?" ([_;value;number], l) -> 
                    Some( (numberOf number.Value, Const( value.Value ))) , l
                | _ -> 
                    failwithf "Could not interpret: '%s' at position %i" (sub v) (position v)

       parseCells parseCell length {input =v; position=0}
            |> List.choose fst

    let rowRegex = Regex(@"
          ^
          (?<columns>[^:]*) \s*
          (
            \: \s* 
            (?<name>\w*) \s* (?<modifier>[+?*]?) \s*
          )?
          $
    ", RegexOptions.IgnorePatternWhitespace)

    [<CompiledName("Rows")>]
    let rows (v:string) : RecognizesRows=
        let m = rowRegex.Match(v)
        let columns =  
            row (m.Groups.["columns"].Value) 

        let name =  m.Groups.["name"].Value
        let modifier = numberOf m.Groups.["modifier"].Value

        (modifier,{recognizer= columns; name= name})

    [<CompiledName("Block")>]
    let block (s : string) : (RecognizesRows list)=
        let rows = s.Split([| '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.filter ( not << String.IsNullOrWhiteSpace)
                        |> Array.map rows 
        Array.toList rows


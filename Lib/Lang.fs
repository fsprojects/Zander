namespace Zander.Internal
open System
open System.Text.RegularExpressions

module Lang=

    open Zander.Internal.String
    open Zander.Internal.Option

    let (|LooksLikeConstant|) (input:StringAndPosition) : (StringAndLength) option=
        opt{
            let! m = regex_match_i "^\"" input
            let! (gs, l) = regex_match_i @".*?(?!\\)""" (s_incr (snd m) input)
            let! g = List.tryHead gs
            return! Some (g.Value.Substring(0,(l-1)), (snd m)+l)
        }
    let number_of (g:string)=
        match g with
            | "" -> One
            | "+" -> Many
            | "*" -> ZeroOrMany
            | "?" -> ZeroOrOne
            | v -> failwithf "Could not interpret: '%s'" v

    [<CompiledName("Row")>]
    let row (v:string) : RecognizesCells list=
       let to_cell (v:StringAndPosition) : ((NumberOf*CellType) option*int)  =
            match v with
                | RegexMatch @"^\s+" ([g], l) -> None, l
                | RegexMatch "^(_)([+*?])?" ([_;_;numberOf], l) -> 
                        Some (number_of numberOf.Value, Empty), l
                | LooksLikeConstant (Some (c, l)) -> 
                        Some((One,Const(c))), l 
                | RegexMatch @"^\@(\w+)([+*?])?" ([_;value;numberOf], l) -> 
                        Some( (number_of numberOf.Value, Value( value.Value ))) , l
                | RegexMatch @"^(\w+)([+*?])?" ([_;value;numberOf], l) -> 
                        Some( (number_of numberOf.Value, Const( value.Value ))) , l
                | _ -> failwithf "Could not interpret: '%s' %i" (sub_i v) (get_position v)

       let rec get_cells input =
            let {input = s; position= i} = input
            let head = to_cell input
            let l = i+ (snd head)
            if l >= s.Length then
                [head]
            else
                head :: get_cells {input=s; position=l} 
       get_cells {input =v; position=0} |> List.choose fst

    let row_regex = new Regex(@"
          ^
          (?<columns>[^:]*) \s*
          (
            \: \s* 
            (?<name>\w*) \s* (?<modifier>[+]?) \s*
          )?
          $
    ", RegexOptions.IgnorePatternWhitespace)

    [<CompiledName("Rows")>]
    let rows (v:string) : RecognizesRows=
        let m = row_regex.Match(v)
        let columns =  
            row (m.Groups.["columns"].Value) 

        let name =  m.Groups.["name"].Value
        let modifier = number_of m.Groups.["modifier"].Value

        { num= modifier; recognizer= columns; name= name}

    [<CompiledName("Block")>]
    let block (s : string) : (RecognizesRows list)=
        let rows = s.Split([| '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.filter ( not << String.IsNullOrWhiteSpace)
                        |> Array.map rows 
        Array.toList rows


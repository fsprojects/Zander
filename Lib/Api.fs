namespace Zander.Internal
open System
open System.Text.RegularExpressions

module Api=

    open Zander.Internal.String
    open Zander.Internal.Option

    let (|LooksLikeConstant|) (input:StringAndPosition) : (StringAndLength) option=
        opt{
            let! m = regex_match_i "^\"" input
            let! (gs, l) = regex_match_i @".*(?!\\)""" (s_incr (snd m) input)
            let! g = List.tryHead gs
            return! Some (g.Value.Substring(0,(l-1)), (snd m)+l)
        }

    let interpret (s : string) : BlockRecognizer=
        ///Match the pattern using a cached compiled Regex

        let to_column (v:StringAndPosition) : (BlockType option*int)  =
            match v with
                | RegexMatch @"^\s+" ([g], l) -> None, l
                | RegexMatch "^_" ([g], l) -> Some Empty, l
                | LooksLikeConstant (Some (c, l)) -> Some(Const(c)), l 
                | RegexMatch @"^\@\w+" ([value], l) -> Some( Value( value.Value.Substring(1) )) , l
                | RegexMatch "^\w+" ([g], l) -> Some( Const(g.Value) ), l
                | _ -> failwithf "Could not interpret! '%s' %i" (sub_i v) (get_position v)

        let rec get_columns input =
            let {input = s; position= i} = input
            let head = to_column input
            let l = i+ (snd head)
            if l >= s.Length then
                [head]
            else
                head :: get_columns {input=s; position=l} 

        let row_regex = new Regex(@"
              ^
              (?<columns>[^:]*) \s*
              \: \s* 
              (?<name>\w+) \s* (?<modifier>[+]?) \s*
              $
        ", RegexOptions.IgnorePatternWhitespace)


        let to_row (v:string) : RowRecognizer=
            let m = row_regex.Match(v)
            let columns =  
                get_columns {input=(m.Groups.["columns"].Value) ; position=0}
                |> List.choose fst 

            let name =  m.Groups.["name"].Value
            let modifier =  
                match m.Groups.["modifier"].Value with
                    | "+" -> NumberOf.Repeat
                    | _ -> NumberOf.Single

            (modifier, columns, name)
            

        let rows = s.Split([| '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.filter ( not << String.IsNullOrWhiteSpace)
                        |> Array.map to_row 
        Array.toList rows


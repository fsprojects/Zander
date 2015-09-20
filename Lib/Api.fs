namespace Zander.Internal
open System
open System.Text.RegularExpressions

module Api=

    open Zander.Internal.String
    open Zander.Internal.Option

    let (|LooksLikeConstant|) (input:string*int) : ((string*int)  ) option=
        opt{
            let! m = regex_match_i "^\"" input
            let! (gs, l) = regex_match_i @".*(?!\\)""" (s_incr (snd m) input)
            let! g = List.tryHead gs
            return! Some (g.Value.Substring(0,(l-1)), (snd m)+l)
        }

    let interpret (s : string) : (NumberOf* BlockType list * string) list=
        ///Match the pattern using a cached compiled Regex

        let to_column (v:string*int) : (BlockType option*int)  =
            match v with
                | RegexMatch @"^\s+" ([g], l) -> None, l
                | RegexMatch "^_" ([g], l) -> Some E, l
                | LooksLikeConstant (Some (c, l)) -> Some(C(c)), l 
                | RegexMatch @"^\@\w+" ([value], l) -> Some( V( value.Value.Substring(1) )) , l
                | _ -> failwithf "! '%s' %i" ((fst v).Substring(snd v)) (snd v)

        let rec get_columns input =
            let (s, i) = input
            let head = to_column input
            let l = i+ (snd head)
            if l >= (fst input).Length then
                [head]
            else
                head :: get_columns (s, l) 

        let row_regex = new Regex(@"
              ^
              (?<columns>[^:]*) \s*
              \: \s* 
              (?<name>\w+) \s* (?<modifier>[+]?) \s*
              $
        ", RegexOptions.IgnorePatternWhitespace)


        let to_row (v:string)=
            let m = row_regex.Match(v)
            let columns =  
                get_columns ((m.Groups.["columns"].Value) ,0) //.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
                |> List.choose fst 

            let name =  m.Groups.["name"].Value
            let modifier =  
                match m.Groups.["modifier"].Value with
                    | "+" -> NumberOf.Repeat
                    | _ -> NumberOf.Single

            (modifier, columns, name)
            

        let rows = s.Split([| '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.map to_row 
        Array.toList rows


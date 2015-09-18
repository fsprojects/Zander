namespace Zander.Internal
open System
open System.Text.RegularExpressions

module Api=
    
    let interpret (s : string) : (NumberOf* BlockType list * string) list=
        
        let to_column (v:string) : BlockType=
            let trimmed = v.Trim([|' '|])
            match trimmed .ToCharArray() |> Array.toList with
                | ['_'] -> E
                | '@'::_ -> V(trimmed.Substring(1))
                | _ -> C(trimmed )

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
                m.Groups.["columns"].Value.Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
                |> Array.map to_column
                |> Array.toList

            let name =  m.Groups.["name"].Value
            let modifier =  
                match m.Groups.["modifier"].Value with
                    | "+" -> NumberOf.Repeat
                    | _ -> NumberOf.Single

            (modifier, columns, name)
            

        let rows = s.Split([| '\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
                        |> Array.map to_row 
        Array.toList rows


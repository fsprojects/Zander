namespace Zander.Internal

type NumberOf=
    | One 
    | ZeroOrOne
    | ZeroOrMany
    | Many
    with
        override self.ToString()=
            match self with
                | One -> "One"
                | Many -> "Many"
                | ZeroOrOne -> "ZeroOrOne"
                | ZeroOrMany -> "ZeroOrMany"

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


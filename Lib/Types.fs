namespace Zander.Internal

type CellType=
    /// empty constant
    | Empty
    /// constant
    | Const of string 
    /// variable with name
    | Value of string 
    with
        override self.ToString()=
            match self with
                | Empty -> "Empty"
                | Const c -> sprintf "'%s'" c
                | Value v -> sprintf "@%s" v

open Zander.Internal.Option

type NumberOf=
    | One 
    | ZeroOrMany
    | Many
    with
        override self.ToString()=
            match self with
                | One -> "One"
                | Many -> "Many"
                | ZeroOrMany -> "ZeroOrMany"


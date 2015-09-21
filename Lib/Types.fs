namespace Zander.Internal

type BlockType=
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

type NumberOf=
    | Single 
    | Repeat 
    with
        override self.ToString()=
            match self with
                | Single -> "Single"
                | Repeat -> "Repeat"
        static member isRepeat num=
            match num with
                | Repeat -> true
                | _ -> false


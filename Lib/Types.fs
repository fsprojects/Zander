namespace Zander.Internal

type BlockType=
    /// empty constant
    | E 
    /// constant
    | C of string 
    /// variable with name
    | V of string 
    with
        override self.ToString()=
            match self with
                | E -> "Empty"
                | C c -> sprintf "'%s'" c
                | V v -> sprintf "@%s" v

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


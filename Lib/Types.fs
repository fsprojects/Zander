namespace Zander.Internal

type BlockType=
    | E // empty constant
    | C of string // constant
    | V of string // variable with name
    with
        override self.ToString()=
            match self with
                | E -> "Empty"
                | C c -> sprintf "'%s'" c
                | V v -> sprintf "@%s" v

type NumberOf=
    | Single // single
    | Repeat  // repeat
    with
        override self.ToString()=
            match self with
                | Single -> "Single"
                | Repeat -> "Repeat"
        static member isRepeat num=
            match num with
                | Repeat -> true
                | _ -> false


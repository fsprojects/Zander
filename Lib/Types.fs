namespace Zander.Internal

type BlockType=
    | E // empty constant
    | C of string // constant
    | V // variable


type NumberOf=
    | Single // single
    | Repeat  // repeat
    with
        static member isRepeat num=
            match num with
                | Repeat -> true
                | _ -> false


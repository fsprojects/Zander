namespace BlockParser

type BlockType=
    | E // empty constant
    | C of string // constant
    | V // variable


type NumberOf=
    | Single // single
    | Repeat  // repeat


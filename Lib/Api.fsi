namespace Zander
type Size={Height:int;Width:int}
type CellType=
    | Unknown=0
    | Constant=1
    | Value=2

[<Class>]
type MatchCell=
    member Success :unit-> bool with get
    member Name: unit-> string with get
    member Value: unit-> string with get
    member CellType: unit-> CellType with get

[<Class>]
type MatchRow=
    member Success :unit-> bool with get
    member Length :unit-> int with get
    member Cells :unit -> MatchCell array with get

type RowEx=
    new : string->RowEx
    member Match : (string array) -> MatchRow

[<Class>]
type MatchBlock=
    member Success :unit-> bool with get
    member Size :unit-> Size with get
    member Rows: unit -> MatchRow array with get
    member WithName: string -> MatchRow array 

type BlockEx=
    new : string->BlockEx
    member Match : (string array array) -> MatchBlock
    //member Match : ((string array array) * int) -> MatchBlock

namespace Zander.Internal

module Lang=
    val internal (|LooksLikeConstant|) : StringAndPosition->  StringAndLength option
    [<CompiledName("Row")>]
    val row : string -> RecognizesCells list
    [<CompiledName("Rows")>]
    val rows : string -> RecognizesRows
    [<CompiledName("Block")>]
    val block : string -> RecognizesRows list
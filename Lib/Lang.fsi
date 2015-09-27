namespace Zander.Internal

module Lang=
    val internal (|LooksLikeConstant|) : StringAndPosition->  StringAndLength option
    val row : string -> RecognizesCells list
    val rows : string -> RecognizesRows
    val block : string -> RecognizesRows list
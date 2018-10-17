namespace Zander.Internal

module Match = 
    [<CompiledName("Expression")>]
    val expression : (Parse.Result list) -> bool
    [<CompiledName("Block")>]
    val block : Parse.RecognizedBlock -> bool

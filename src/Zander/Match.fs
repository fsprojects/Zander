namespace Zander.Internal

module Match = 
    
    [<CompiledName("Expression")>]
    let expression recognized=
        recognized
            |> List.forall Result.isOk

    [<CompiledName("Block")>]
    let block (recognized:RecognizedBlock) =
        recognized
            |> List.map fst
            |> List.collect id
            |> List.forall Result.isOk

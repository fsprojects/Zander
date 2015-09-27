namespace Zander.Internal

module Match = 
    
    [<CompiledName("Expression")>]
    let expression recognized=
        recognized
            |> List.forall Parse.Result.isOk

    [<CompiledName("Block")>]
    let block (recognized:Parse.RecognizedBlock) =
        recognized
            |> List.map fst
            |> List.collect id
            |> List.forall Parse.Result.isOk

namespace Zander.Internal

module Match = 
    
    let expression (recognized: Parse.Result list) : bool=
        recognized
            |> List.forall Parse.Result.isOk

    let block (recognized:Parse.RecognizedBlock) : bool=
        recognized
            |> List.map fst
            |> List.collect id
            |> List.forall Parse.Result.isOk

namespace Zander.Internal

module Match = 
    
    let expression rowExpr row : bool=
        Parse.expression rowExpr row
            |> List.forall Parse.Result.isOk

    let block expr index blocks : bool=
        Parse.block expr index blocks
            |> List.map fst
            |> List.collect id
            |> List.forall Parse.Result.isOk

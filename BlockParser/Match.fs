namespace BlockParser

module Match = 
    
    let expression rowExpr row=
        Parse.expression rowExpr row
            |> List.forall Parse.Result.isOk

    let block expr index blocks =
        Parse.block expr index blocks
            |> List.forall Parse.Result.isOk

namespace BlockParser

module Match = 
    
    let expression rowExpr row=
        Parse.expression rowExpr row
            |> List.forall Parse.Result.isOk

    let block expr index blocks =

        let rec bmatch idx eidx=
            let erow = expr |> List.tryItem eidx
            let row = blocks |> List.tryItem (index + idx)

            match erow, row with
                | Some (c,e,n), Some r -> 
                    ( expression e r ) && ( 
                        ( (NumberOf.isRepeat c) && bmatch (idx+1) eidx ) 
                            ||  ( bmatch (idx+1) (eidx+1) )
                        )
                | None, None -> true
                | _, _ -> false
        
        bmatch 0 0

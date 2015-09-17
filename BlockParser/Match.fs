namespace BlockParser

module Match = 
    
    let expression rowExpr row=
        let is_ok v=
            match v with 
                | Parse.Result.Ok _ -> true
                | _ -> false

        Parse.expression rowExpr row
            |> List.forall is_ok

    let block expr index blocks =
        let repeat_expression c= // can this be done better?
            match c with
                | Repeat -> true
                | _ -> false

        let rec bmatch idx eidx=
            let erow = expr |> List.tryItem eidx
            let row = blocks |> List.tryItem (index + idx)

            match erow, row with
                | Some (c,e,n), Some r -> 
                    ( expression e r ) && ( 
                        ( (repeat_expression c) && bmatch (idx+1) eidx ) 
                            ||  ( bmatch (idx+1) (eidx+1) )
                        )
                | None, None -> true
                | _, _ -> false
        
        bmatch 0 0

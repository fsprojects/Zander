namespace BlockParser

module Match = 

    let expression rowExpr row=
        let columnMatch columnExpr column=
            match columnExpr, column with
                | E, "" -> true
                | C v1,v2 -> v1 = v2
                | V _,_ -> true
                | _,_ -> false 

        if (List.length rowExpr) <> (List.length row) then
            false
        else
            let r = List.map2 columnMatch rowExpr row 
            r |> List.forall id

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

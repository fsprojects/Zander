namespace Zander
open System

module Parse=
    type Token=
        | Value of string
        | Constant
        | Empty
        with
            static member value t=
                match t with
                    | Value v-> Some v
                    | _ -> None

    type Result=
        | Ok of Token
        | WrongConstant of (string * string)
        | UnRecognized of string
        | Missing 
        with
            static member isOk result=
                match result with
                    | Ok v -> true
                    | _ -> false
            static member value result=
                match result with
                    | Ok v -> Some v
                    | _ -> None


    let expression rowExpr row : Result list=
        let columnMatch columnExpr column=

            match columnExpr, column with
                | E, "" -> Ok( Empty)
                | C v1,v2 -> 
                    if v1=v2 then 
                        Ok( Constant )
                    else
                        WrongConstant (v1, v2)
                | V , v -> Ok( Value v )
                | _, v-> UnRecognized v

        if (List.length rowExpr) <> (List.length row) then
            row |> List.map UnRecognized
        else
            List.map2 columnMatch rowExpr row 
            
    let block expr index blocks : ((Result list*string) list)=

        let rec bmatch idx eidx : (Result list*string) list=
            let erow = expr |> List.tryItem eidx
            let row = blocks |> List.tryItem (index + idx)

            let is_ok (r : (Result list*string) list)=
                r |> List.map fst
                  |> List.collect id
                  |> List.forall Result.isOk

            let rest_of c e n : (Result list*string) list=
                let r = bmatch (idx+1) eidx 
                if NumberOf.isRepeat c && is_ok r then
                    r
                else
                    bmatch (idx+1) (eidx+1)

            match erow, row with
                | Some (c,e,n), Some r -> 
                    let h = [((expression e r), n)]
                    let r = rest_of c e n
                    h @ r
                | None, None -> []
                | Some (_,_,n), None -> [[Missing], n]
                | None, Some r -> []
        
        bmatch 0 0

    let rowsOf v = 
        let valuesOf v' =
            v'
            |> List.map Result.value
            |> List.choose id
            |> List.map Token.value
            |> List.choose id

        v |> List.map (
             fun (row,name)-> (valuesOf row) , name
             )
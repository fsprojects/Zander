namespace BlockParser
open System

module Parse=
    type Token=
        | Value of string
        | Constant
        | Empty

    type Result=
        | Ok of Token
        | WrongConstant of (string * string)
        | UnRecognized of string
    
    let expression rowExpr row=
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
            

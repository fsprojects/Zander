namespace Tests
open NUnit.Framework
open FsUnit
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module RowParseTests = 

    open Parse
    
    let private valuesOfExpression v a= 
        s_expression v a 
            |> List.map Result.value
            |> List.choose Token.tryValue


    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        valuesOfExpression [Empty] [""] |> should equal []

    [<Test>] 
    let ``Single column match constant expression`` ()=
        valuesOfExpression  [Const "1"] ["1"] |> should equal []

    [<Test>] 
    let ``Single column should match variable`` ()=
        valuesOfExpression [Value ""] ["2"] |> should equal ["2"]

    [<Test>] 
    let ``Should match more complicated example`` ()=
        valuesOfExpression  
            [Empty; Const "1"; Value "" ; Const "2" ] 
            [""; "1"; "X"; "2" ]  
                |> should equal ["X"]

namespace Tests
open NUnit.Framework
open FsUnit
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module RowParseTests = 

    open Parse
    let private valuesOfExpression v = 
        v
            |> List.map Result.value
            |> List.choose Token.tryValue

    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        valuesOfExpression (expression [Empty] [""]) |> should equal []

    [<Test>] 
    let ``Single column match constant expression`` ()=
        valuesOfExpression (expression [Const "1"] ["1"]) |> should equal []

    [<Test>] 
    let ``Single column should match variable`` ()=
        valuesOfExpression (expression [Value ""] ["2"]) |> should equal ["2"]

    [<Test>] 
    let ``Single empty column should match variable`` ()=
        valuesOfExpression ( expression [Value ""] [""]) |> should equal [""]

    [<Test>] 
    let ``Should match more complicated example`` ()=
        valuesOfExpression ( expression [Empty; Const "1"; Value "" ; Const "2" ] [""; "1"; "X"; "2" ] ) |> should equal ["X"]

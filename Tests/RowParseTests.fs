namespace Tests
open NUnit.Framework
open FsUnit
open Zander

[<TestFixture>] 
module RowParseTests = 

    open Parse
    let valuesOfExpression v = 
        v
            |> List.map Result.value
            |> List.choose id
            |> List.map Token.value
            |> List.choose id

    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        valuesOfExpression (expression [E] [""]) |> should equal []

    [<Test>] 
    let ``Single column match constant expression`` ()=
        valuesOfExpression (expression [C "1"] ["1"]) |> should equal []

    [<Test>] 
    let ``Single column should match variable`` ()=
        valuesOfExpression (expression [V] ["2"]) |> should equal ["2"]

    [<Test>] 
    let ``Single empty column should match variable`` ()=
        valuesOfExpression ( expression [V] [""]) |> should equal [""]

    [<Test>] 
    let ``Should match more complicated example`` ()=
        valuesOfExpression ( expression [E; C "1"; V ; C "2" ] [""; "1"; "X"; "2" ] ) |> should equal ["X"]

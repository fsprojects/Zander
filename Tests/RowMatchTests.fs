namespace Tests
open NUnit.Framework
open FsUnit
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module RowMatchTests = 

    open Match

    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        expression [Empty] [""] |> should equal true

    [<Test>] 
    let ``Single non empty column should not match empty expression`` ()=
        expression [Empty] ["1"] |> should equal false

    [<Test>] 
    let ``Single column match constant expression`` ()=
        expression [Const "1"] ["1"] |> should equal true

    [<Test>] 
    let ``Single column should not match constant expression`` ()=
        expression [Const "1"] ["2"] |> should equal false

    [<Test>] 
    let ``Single column should match variable`` ()=
        expression [Value ""] ["2"] |> should equal true

    [<Test>] 
    let ``Single empty column should match variable`` ()=
        expression [Value ""] [""] |> should equal true

    [<Test>] 
    let ``Should match more complicated example`` ()=
        expression [Empty; Const "1"; Value ""; Const "2" ] [""; "1"; "X"; "2" ] |> should equal true

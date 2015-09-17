namespace Tests
open NUnit.Framework
open FsUnit
open BlockParser

[<TestFixture>] 
module RowMatchTests = 

    open Match

    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        expression [E] [""] |> should equal true

    [<Test>] 
    let ``Single non empty column should not match empty expression`` ()=
        expression [E] ["1"] |> should equal false

    [<Test>] 
    let ``Single column match constant expression`` ()=
        expression [C "1"] ["1"] |> should equal true

    [<Test>] 
    let ``Single column should not match constant expression`` ()=
        expression [C "1"] ["2"] |> should equal false

    [<Test>] 
    let ``Single column should match variable`` ()=
        expression [V] ["2"] |> should equal true

    [<Test>] 
    let ``Single empty column should match variable`` ()=
        expression [V] [""] |> should equal true

    [<Test>] 
    let ``Should match more complicated example`` ()=
        expression [E; C "1"; V ; C "2" ] [""; "1"; "X"; "2" ] |> should equal true

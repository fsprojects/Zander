namespace Tests
open NUnit.Framework
open FsUnit
open BlockParser

[<TestFixture>] 
module RowParseTests = 

    open Parse

    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        expression [E] [""] |> should equal []

    [<Test>] 
    let ``Single column match constant expression`` ()=
        expression [C "1"] ["1"] |> should equal []

    [<Test>] 
    let ``Single column should match variable`` ()=
        expression [V] ["2"] |> should equal ["2"]

    [<Test>] 
    let ``Single empty column should match variable`` ()=
        expression [V] [""] |> should equal [""]

    [<Test>] 
    let ``Should match more complicated example`` ()=
        expression [E; C "1"; V ; C "2" ] [""; "1"; "X"; "2" ] |> should equal ["X"]

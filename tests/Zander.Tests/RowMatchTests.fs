namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander.Internal
open TestHelpers

module RowMatchTests = 

    open Zander

    [<Fact>] 
    let ``Single empty column match empty expression`` ()=
        matchSExpression [Empty] [""] |> should equal true

    [<Fact>] 
    let ``several empty column match repeat empty expression`` ()=
        parseAndMatchExpression [Many,Empty] ["";""] |> should equal true

    [<Fact>] 
    let ``Single non empty column should not match empty expression`` ()=
        matchSExpression [Empty] ["1"] |> should equal false

    [<Fact>] 
    let ``Single column match constant expression`` ()=
        matchSExpression [Const "1"] ["1"] |> should equal true

    [<Fact>] 
    let ``Single column should not match constant expression`` ()=
        matchSExpression [Const "1"] ["2"] |> should equal false

    [<Fact>] 
    let ``Single column should match variable`` ()=
        matchSExpression [Value ""] ["2"] |> should equal true

    [<Fact>] 
    let ``several column should match repeat variable`` ()=
        parseAndMatchExpression [Many,Value ""] ["1";"2"] |> should equal true

    [<Fact>] 
    let ``Single empty column should not match variable`` ()=
        matchSExpression [Value ""] [""] |> should equal false

    [<Fact>] 
    let ``Single empty column should match variable or empty`` ()=
        matchSExpression [Or [ Value ""; Empty ]] [""] |> should equal true

    [<Fact>] 
    let ``Single column should match variable or empty`` ()=
        matchSExpression [Or [ Value ""; Empty ]] ["1"] |> should equal true

    [<Fact>]
    let ``Should match if option is specified to match empty`` ()=
        matchSExpressionOpt [Value ""] ParseOptions.ValueMatchesEmpty [""] |> should equal true

    [<Fact>] 
    let ``Should match more complicated example`` ()=
        matchSExpression [Empty; Const "1"; Value ""; Const "2" ] [""; "1"; "X"; "2" ] |> should equal true

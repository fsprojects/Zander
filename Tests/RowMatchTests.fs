namespace Tests
open NUnit.Framework
open FsUnit
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module RowMatchTests = 

    open Match
    open Zander

    [<Test>] 
    let ``Single empty column match empty expression`` ()=
        match_s_expression [Empty] [""] |> should equal true

    [<Test>] 
    let ``several empty column match repeat empty expression`` ()=
        parse_and_match_expression [Many,Empty] ["";""] |> should equal true

    [<Test>] 
    let ``Single non empty column should not match empty expression`` ()=
        match_s_expression [Empty] ["1"] |> should equal false

    [<Test>] 
    let ``Single column match constant expression`` ()=
        match_s_expression [Const "1"] ["1"] |> should equal true

    [<Test>] 
    let ``Single column should not match constant expression`` ()=
        match_s_expression [Const "1"] ["2"] |> should equal false

    [<Test>] 
    let ``Single column should match variable`` ()=
        match_s_expression [Value ""] ["2"] |> should equal true

    [<Test>] 
    let ``several column should match repeat variable`` ()=
        parse_and_match_expression [Many,Value ""] ["1";"2"] |> should equal true

    [<Test>] 
    let ``Single empty column should not match variable`` ()=
        match_s_expression [Value ""] [""] |> should equal false

    [<Test>] 
    let ``Single empty column should match variable or empty`` ()=
        match_s_expression [Or [ Value ""; Empty ]] [""] |> should equal true

    [<Test>] 
    let ``Single column should match variable or empty`` ()=
        match_s_expression [Or [ Value ""; Empty ]] ["1"] |> should equal true

    [<Test>]
    let ``Should match if option is specified to match empty`` ()=
        match_s_expression_opt [Value ""] ParseOptions.ValueMatchesEmpty [""] |> should equal true

    [<Test>] 
    let ``Should match more complicated example`` ()=
        match_s_expression [Empty; Const "1"; Value ""; Const "2" ] [""; "1"; "X"; "2" ] |> should equal true

namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander.Internal
open TestHelpers

module RowParseTests = 

    [<Fact>] 
    let ``Single empty column match empty expression`` ()=
        Assert.Empty (valuesOfExpression [Empty] [""])

    [<Fact>] 
    let ``Single column match constant expression`` ()=
        Assert.Empty (valuesOfExpression [Const "1"] ["1"])

    [<Fact>] 
    let ``Single column should match variable`` ()=
        valuesOfExpression [Value ""] ["2"] |> should equal ["2"]

    [<Fact>] 
    let ``Single column should match variable or empty`` ()=
        valuesOfExpression [Or [ Value ""; Empty ]] ["2"] |> should equal ["2"]

    [<Fact>] 
    let ``Should match more complicated example`` ()=
        valuesOfExpression  
            [Empty; Const "1"; Value "" ; Const "2" ] 
            [""; "1"; "X"; "2" ]  
                |> should equal ["X"]

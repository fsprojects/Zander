namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal

[<TestFixture>] 
module InterpretFormatTests = 

    [<Test>] 
    let ``Can parse single row expression`` ()=
        let expression = [Single, ([Empty; Value "Title"]), "header"]
        let apiCode =    " _   @Title : header"
        Api.interpret apiCode |> should equal expression

    [<Test>] 
    let ``Can parse constant`` ()=
        let expression = [Single, ([Empty; Const "constant"]), "header"]
        let apiCode =    " _   constant : header"
        Api.interpret apiCode |> should equal expression

    [<Test>] 
    let ``Can parse row expression with constant within "`` ()=
        let expression = [Single, ([Empty; Const "Some constant"]), "header"]
        let apiCode =    " _   \"Some constant\" : header"
        Api.interpret apiCode |> should equal expression


    [<Test>]
    let ``Regression test for constants`` ()=
        let apiCode = @"""Attribute 1"" _ ""Attribute 2"": header"
        let expression = [Single, ([Const "Attribute 1"; Empty; Const "Attribute 2"; ]), "header"]
        Api.interpret apiCode |> should equal expression


    [<Test>] 
    let ``Can parse`` ()=
        let expression = [
                        Single, ([Empty; Value "Header"]), "header"
                        Repeat, ([Value "Data"; Empty]), "data_rows"
                    ]
        let apiCode =    " _   @Header : header 
                          @Data   _ : data_rows+"
        Api.interpret apiCode |> should equal expression
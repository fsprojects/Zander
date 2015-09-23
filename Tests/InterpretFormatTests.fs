namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal

[<TestFixture>] 
module InterpretFormatTests = 

    [<Test>] 
    let ``Can parse single row expression`` ()=
        let expression = [One, ([One,Empty; One,Value "Title"]), "header"]
        let apiCode =    " _   @Title : header"
        Api.interpret apiCode |> should equal expression

    [<Test>] 
    let ``Can parse constant`` ()=
        let expression = [One, ([One,Empty; One,Const "constant"]), "header"]
        let apiCode =    " _   constant : header"
        Api.interpret apiCode |> should equal expression

    [<Test>] 
    let ``Can parse row expression with constant within "`` ()=
        let expression = [One, ([One,Empty; One,Const "Some constant"]), "header"]
        let apiCode =    " _   \"Some constant\" : header"
        Api.interpret apiCode |> should equal expression


    [<Test>]
    let ``Regression test for constants`` ()=
        let apiCode = @"""Attribute 1"" _ ""Attribute 2"": header"
        let expression = [One, ([One,Const "Attribute 1"; One,Empty; One,Const "Attribute 2"; ]), "header"]
        Api.interpret apiCode |> should equal expression


    [<Test>] 
    let ``Can parse`` ()=
        let expression = [
                        One, ([One,Empty; One,Value "Header"]), "header"
                        Many, ([One,Value "Data"; One,Empty]), "data_rows"
                    ]
        let apiCode =    " _   @Header : header 
                          @Data   _ : data_rows+"
        Api.interpret apiCode |> should equal expression
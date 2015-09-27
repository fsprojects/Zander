namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal

[<TestFixture>] 
module InterpretFormatTests = 

    [<Test>] 
    let ``Can parse single row expression`` ()=
        let expression = [{num=One; recognizer=([One,Empty; One,Value "Title"]);name= "header"}]
        let apiCode =    " _   @Title : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse constant`` ()=
        let expression = [{num=One;recognizer= ([One,Empty; One,Const "constant"]);name= "header"}]
        let apiCode =    " _   constant : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse row expression with constant within "`` ()=
        let expression = [{num=One;recognizer= ([One,Empty; One,Const "Some constant"]); name= "header"}]
        let apiCode =    " _   \"Some constant\" : header"
        Lang.block apiCode |> should equal expression


    [<Test>]
    let ``Regression test for constants`` ()=
        let apiCode = @"""Attribute 1"" _ ""Attribute 2"": header"
        let expression = [{num=One; recognizer= ([One,Const "Attribute 1"; One,Empty; One,Const "Attribute 2"; ]);name= "header"}]
        Lang.block apiCode |> should equal expression


    [<Test>] 
    let ``Can parse`` ()=
        let expression = [
                        {num=One; recognizer=([One,Empty; One,Value "Header"]);name="header"}
                        {num=Many; recognizer=([One,Value "Data"; One,Empty]); name="data_rows"}
                    ]
        let apiCode =    " _   @Header : header 
                          @Data   _ : data_rows+"
        Lang.block apiCode |> should equal expression
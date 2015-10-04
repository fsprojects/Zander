namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal

[<TestFixture>] 
module InterpretFormatTests = 

    [<Test>] 
    let ``Can parse single row expression`` ()=
        let expression = [(One,{recognizer=([One,Empty; One,Value "Title"]);name= "header"})]
        let apiCode =    " _   @Title : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse optional row expression`` ()=
        let expression = [(ZeroOrOne,{recognizer=([One,Empty; One,Value "Title"]);name= "header"})]
        let apiCode =    " _   @Title : header?"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse * row expression`` ()=
        let expression = [(ZeroOrMany,{ recognizer=([One,Empty; One,Value "Title"]);name= "header"})]
        let apiCode =    " _   @Title : header*"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse constant`` ()=
        let expression = [(One,{recognizer= ([One,Empty; One,Const "constant"]);name= "header"})]
        let apiCode =    " _   constant : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse row expression with constant within "`` ()=
        let expression = [(One,{recognizer= ([One,Empty; One,Const "Some constant"]); name= "header"})]
        let apiCode =    " _   \"Some constant\" : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse 'or' constant, empty ``()=
        let expression = [(One,{recognizer= ([One,Or [Const "Some constant"; Empty]]); name= "header"})]
        let apiCode =    " (\"Some constant\"|_) : header"
        Lang.block apiCode |> should equal expression

        let apiCode =    " ( \"Some constant\" | _ ) : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse 'or' many different constants``()=
        let expression = [(One,{recognizer= ([One,Or [Const "A 1"; Const "B 1"; Const "C"]]); name= "header"})]
        let apiCode =    " (\"A 1\"|\"B 1\"|C) : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse 'or' constants with | in the constant``()=
        let expression = [(One,{recognizer= ([One,Or [Const "A | 1"]]); name= "header"})]
        let apiCode =    " (\"A | 1\") : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse 'or' constants with " in the constant``()=
        let expression = [(One,{recognizer= ([One,Or [Const @"A \"" 1"]]); name= "header"})]
        let apiCode =    @" (""A \"" 1"") : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse constants with " in the constant``()=
        let expression = [(One,{recognizer= ([One,Const @"A \"" 1"]); name= "header"})]
        let apiCode =    @"""A \"" 1"" : header"
        Lang.block apiCode |> should equal expression

    [<Test>] 
    let ``Can parse or value, empty ``()=
        let expression = [(One,{recognizer= ([One,Or[Value "A";Empty]]); name= "header"})]
        let apiCode =    " (@A|_) : header"
        Lang.block apiCode |> should equal expression

        let apiCode =    " ( @A | _ ) : header"
        Lang.block apiCode |> should equal expression


    [<Test>]
    let ``Regression test for constants`` ()=
        let apiCode = @"""Attribute 1"" _ ""Attribute 2"": header"
        let expression = [(One,{ recognizer= ([One,Const "Attribute 1"; One,Empty; One,Const "Attribute 2"; ]);name= "header"})]
        Lang.block apiCode |> should equal expression


    [<Test>] 
    let ``Can parse`` ()=
        let expression = [
                        (One,{ recognizer=([One,Empty; One,Value "Header"]);name="header"})
                        (Many,{ recognizer=([One,Value "Data"; One,Empty]); name="data_rows"})
                    ]
        let apiCode =    " _   @Header : header 
                          @Data   _ : data_rows+"
        Lang.block apiCode |> should equal expression
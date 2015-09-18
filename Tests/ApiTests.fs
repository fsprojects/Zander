namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal

[<TestFixture>] 
module ApiTests = 

    [<Test>] 
    let ``Can parse single row expression`` ()=
        let expression = [Single, ([E; V]), "header"]
        let apiCode =    " _   @V : header"
        Api.interpret apiCode |> should equal expression


    [<Test>] 
    let ``Can parse`` ()=
        let expression = [
                        Single, ([E; V]), "header"
                        Repeat, ([V; E]), "data_rows"
                    ]
        let apiCode =    " _   @V : header 
                          @V   _ : data_rows+"
        Api.interpret apiCode |> should equal expression
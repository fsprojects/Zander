namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal

[<TestFixture>] 
module ApiTests = 

    [<Test>] 
    let ``Can parse single row expression`` ()=
        let expression = [Single, ([E; V "Title"]), "header"]
        let apiCode =    " _   @Title : header"
        Api.interpret apiCode |> should equal expression


    [<Test>] 
    let ``Can parse`` ()=
        let expression = [
                        Single, ([E; V "Header"]), "header"
                        Repeat, ([V "Data"; E]), "data_rows"
                    ]
        let apiCode =    " _   @Header : header 
                          @Data   _ : data_rows+"
        Api.interpret apiCode |> should equal expression
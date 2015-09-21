namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module BuilderTests = 

    let builder = new Builder()
    let first_expression = [
                        Single, ([Empty; Value ""]), "header"
                        Repeat, ([Value ""; Empty]), "data_rows"
                    ]
    let second_expression = [Repeat, ([Empty; Value ""]), "data_rows2" ]

    let spec input= 
           builder
                .RawBlock( ("fst", first_expression) )
                .RawBlock( ("snd",second_expression) )
                .Parse( input )

    let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]

    let expected_first_part = 
        {Name = "fst" ; Rows = [|
                                 {Name= "header"; Values= [| kv "" "H"|]}
                                 {Name="data_rows";Values= [|kv "" "D1"|]}
                                 {Name="data_rows";Values= [|kv "" "D2"|]}
                               |]
                    }
        
    let expected_second_part =  
        { Name = "snd"; Rows= [|
                                {Name= "data_rows2"; Values= [|kv "" "D3"|]}
                               |] }

    [<Test>] 
    let ``Can parse first part`` ()=
        ( Match.block first_expression 0 sections ) |> should equal true

        ( Parse.block first_expression 0 sections ) 
                 |> List.length
                 |> should equal 3

    [<Test>] 
    let ``Cant parse second part with first expression`` ()=
        ( Match.block first_expression 3 sections ) |> should equal false

    [<Test>] 
    let ``Can parse second part with second expression`` ()=
        ( Match.block second_expression 3 sections ) |> should equal true


    [<Test>] 
    let ``Can parse complex example`` ()=
        let expected =  
           [
               expected_first_part
               expected_second_part
           ]
        let s =
             sections |> List.map List.toArray
                      |> List.toArray
        
        (spec s) |> should equal expected
    
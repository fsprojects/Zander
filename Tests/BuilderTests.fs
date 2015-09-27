namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module BuilderTests = 

    let builder = new ParserBuilder()
    let first_expression = [
                        {num=One; recognizer= ([One,Empty; One,Value ""]); name= "header"}
                        {num=Many; recognizer= ([One,Value ""; One,Empty]); name= "data_rows"}
                    ]
    let second_expression = [{num=Many; recognizer=([One,Empty; One,Value ""]); name= "data_rows2"} ]

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
    open TestHelpers
    [<Test>] 
    let ``Can parse first part`` ()=
        ( parse_and_match_block first_expression 0 sections ) |> should equal true

        ( Parse.block first_expression 0 sections ) 
                 |> List.length
                 |> should equal 3

    [<Test>] 
    let ``Cant parse second part with first expression`` ()=
        ( parse_and_match_block first_expression 3 sections ) |> should equal false

    [<Test>] 
    let ``Can parse second part with second expression`` ()=
        ( parse_and_match_block second_expression 3 sections ) |> should equal true


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
    
namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

module BuilderTests = 
    let first_expression = [
                        (One,{recognizer= ([One,Empty; One,Value ""]); name= "header"})
                        (Many,{recognizer= ([One,Value ""; One,Empty]); name= "data_rows"})
                    ]
    let second_expression = [(Many,{recognizer=([One,Empty; One,Value ""]); name= "data_rows2"})]

    let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]

    let expected_first_part = 
        {Name = "fst" ; Rows = [|
                                 {Name= "header"; Values= [| kv "" "H"|]}
                                 {Name="data_rows";Values= [|kv "" "D1"|]}
                                 {Name="data_rows";Values= [|kv "" "D2"|]}
                               |]}
        
    let expected_second_part =  
        { Name = "snd"; Rows= [|
                                {Name= "data_rows2"; Values= [|kv "" "D3"|]}
                               |] }
    open TestHelpers
    [<Fact>] 
    let ``Can parse first part when block may have partial match`` ()=
        parse_and_match_block first_expression sections |> should equal true

        Parse.block first_expression opts (sections |> List.take 3)
                 |> List.length
                 |> should equal 3

    [<Fact>] 
    let ``Can parse first part (complete match)`` ()=
        Parse.block first_expression ParseOptions.BlockMatchesAll (sections |> List.take 3)
                 |> List.length
                 |> should equal 3

        Parse.block first_expression ParseOptions.BlockMatchesAll sections 
                 |> Match.block
                 |> should equal false

    [<Fact>] 
    let ``Cant parse second part with first expression`` ()=
        ( parse_and_match_block first_expression (sections |> List.skip 3)) |> should equal false

    [<Fact>] 
    let ``Can parse second part with second expression`` ()=
        ( parse_and_match_block second_expression (sections |> List.skip 3) ) |> should equal true


    [<Fact>] 
    let ``Can parse complex example`` ()=
        let spec input= 
           (new ParserBuilder())
                .RawBlock( ("fst", first_expression) )
                .RawBlock( ("snd",second_expression) )
                .Parse( input )

        let expected =  
           [
               expected_first_part
               expected_second_part
           ]
        let s =
             sections |> List.map List.toArray
                      |> List.toArray
        
        (spec s) |> should equal expected
    
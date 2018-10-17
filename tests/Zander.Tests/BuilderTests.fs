namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

module BuilderTests = 
    open Zander.Internal
    let okKv k v=Token.createValue (k, v) |> Result.Ok |> MatchCell
    let okC k v=Token.createConstant (k, v) |> Result.Ok |> MatchCell 
    let toArrayArray ll = ll |> List.map List.toArray |> List.toArray
    let firstExpressionStr = @"_ @_ : header
                               @_ _ : data_rows+"
    let first_expression = [
                        (One,{recognizer= ([One,Empty; One,Value "_"]); name= "header"})
                        (Many,{recognizer= ([One,Value "_"; One,Empty]); name= "data_rows"})
                    ]
    let secondExpressionStr = @"_ @_ : data_rows2+"
    let second_expression = [(Many,{recognizer=([One,Empty; One,Value "_"]); name= "data_rows2"})]

    let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]

    let expected_first_part = 
        {Name = "fst" ; Rows = [|
                                 {Name= "header"; Cells= [|okC "" "" ; okKv "_" "H"|]}
                                 {Name="data_rows"; Cells= [|okKv "_" "D1"; okC "" "" |]}
                                 {Name="data_rows"; Cells= [|okKv "_" "D2"; okC "" "" |]}
                               |]}
        
    let expected_second_part =  
        { Name = "snd"; Rows= [|
                                {Name= "data_rows2"; Cells= [|okC "" ""; okKv "_" "D3"|]}
                               |] }
    open TestHelpers
    [<Fact>] 
    let ``Can parse first part when block may have partial match`` ()=
        parseAndMatchBlock first_expression sections |> should equal true
        Block.parse first_expression opts (sections |> List.take 3)
                 |> List.length
                 |> should equal 3
        let b= BlockEx(firstExpressionStr)
        let m =b.Match(sections |> toArrayArray) 
        m.Success |> should equal true

    [<Fact>] 
    let ``Can parse first part (complete match)`` ()=
        Block.parse first_expression ParseOptions.BlockMatchesAll (sections |> List.take 3)
                 |> List.length
                 |> should equal 3

        Block.parse first_expression ParseOptions.BlockMatchesAll sections 
                 |> Block.isMatch
                 |> should equal false

    [<Fact>] 
    let ``Cant parse second part with first expression`` ()=
        ( parseAndMatchBlock first_expression (sections |> List.skip 3)) |> should equal false

    [<Fact>] 
    let ``Can parse second part with second expression`` ()=
        ( parseAndMatchBlock second_expression (sections |> List.skip 3) ) |> should equal true


    [<Fact>] 
    let ``Can parse complex example`` ()=
        let spec input= 
           ParserBuilder()
                .Block( ("fst", firstExpressionStr) )
                .Block( ("snd", secondExpressionStr) )
                .Parse( input )

        let expected =  
           [
               expected_first_part
               expected_second_part
           ]
        let s =
             sections |> List.map List.toArray
                      |> List.toArray
        
        Assert.Equal<ParsedBlock list> (expected, (spec s)|> Seq.toList)
    
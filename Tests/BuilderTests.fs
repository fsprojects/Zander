namespace Tests
open NUnit.Framework
open FsUnit
open Zander

[<TestFixture>] 
module BuilderTests = 
    type Builder()=
        let mutable array : (NumberOf* BlockType list * string ) list list= [];

        member this.Block(x : (NumberOf* BlockType list * string) list ) = 
            array <- array @ [ x ]
            this

        member this.Parse(blocks : string list list) =
            let rec parse index =
                if index >= List.length blocks then
                    []
                else
                    let maybeNext =  array |> List.tryFind (fun sp-> (Match.block sp index blocks ) )
                    match maybeNext with
                        | Some next -> 
                            let parsed = Parse.block next index blocks
                            let nextIndex = index + (List.length parsed)
                            [ parsed ] @ (parse nextIndex) 
                        | None -> 
                            (failwithf "could not find expression block for index %i" index)

            parse 0

    let builder = new Builder()
    let first_expression = [
                        Single, ([E; V]), "header"
                        Repeat, ([V; E]), "data_rows"
                    ]
    let second_expression = [Repeat, ([E; V]), "data_rows2" ]

    let spec input= 
           builder
                .Block(first_expression).Block(second_expression).Parse( input )
    let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]

    let expected_first_part = 
        [
          (["H"],"header")
          (["D1"],"data_rows")
          (["D2"],"data_rows")
        ]
    let expected_second_part =  
           [
            (["D3"],"data_rows2")
           ]

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
        ( (spec sections) |> List.map Parse.rowsOf ) |> should equal expected
    
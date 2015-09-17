namespace Tests
open NUnit.Framework
open FsUnit
open BlockParser

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
                    let next =  array |> List.find (fun sp-> (Match.block sp index blocks ) )
                    let parsed = Parse.block next index blocks
                    let nextIndex = parsed |> List.length
                    [ parsed ] @ (parse nextIndex) 
            parse 0

    let builder = new Builder()

    let spec input= 
           builder
                .Block([Single, ([E; V]), "header"
                        Repeat, ([V; E]), "data_rows"
                    ]).Block([Repeat, ([E; V]), "data_rows2" ]).Parse( input )
     

    [<Test>] 
    let ``Can parse complex example`` ()=
        let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]
        let expected =  
           [
               [
                (["H"],"header")
                (["D1"],"data_rows")
                (["D2"],"data_rows")
               ]
               [
                (["D3"],"data_rows2")
               ]
           ]
        ( (spec sections) |> List.map Parse.rowsOf ) |> should equal expected
    
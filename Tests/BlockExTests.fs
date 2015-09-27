namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module BlockExTests = 
    let getValues (m:MatchBlock)=
             m.Rows 
                |> Array.collect (fun r->r.Cells)
                |> Array.filter (fun c->c.CellType = Zander.CellType.Value)
                |> Array.map (fun c-> (c.Name, c.Value))

    [<Test>] 
    let ``Can parse complex example`` ()=
        let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]
                        |> List.map List.toArray
                        |> List.toArray
        let blockEx = new BlockEx("_ \"H\" : header
        @D _ : data_rows+
        _ @D : data_rows2+
        ")
        let m = blockEx.Match(sections)
        m.Success |> should equal true
        m.Size |> should equal {Height=4;Width=2};
        getValues m |> should equal [| ("D","D1"); ("D","D2"); ("D","D3") |]
        m.WithName "data_rows" |> Array.length |> should equal 2
        m.WithName "data_rows2" |> Array.length |> should equal 1

    [<Test>]
    let ``Can specify simple 2d block``() =
        let sections = [["";"H"];["D1";""];]
                        |> List.map List.toArray
                        |> List.toArray
        let blockEx = new BlockEx("_ \"H\"
        @D _ 
        ")
        let m = blockEx.Match(sections)
        m.Success |> should equal true
        m.Size |> should equal {Height=2;Width=2};
        getValues m |> should equal [| ("D","D1"); |]

    [<Test>]
    let ``Can specify simple 2d block with repeated row``() =
        let sections = [["";"H"];["D1";""];["D2";""];]
                        |> List.map List.toArray
                        |> List.toArray
        let blockEx = new BlockEx("_ \"H\"
        @D _ :+
        ")
        let m = blockEx.Match(sections)
        m.Success |> should equal true
        m.Size |> should equal {Height=3;Width=2};
        getValues m |> should equal [| ("D","D1"); ("D","D2");|]


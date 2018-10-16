namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

module BlockExTests = 
    let getValues (m:MatchBlock)=
             m.Rows 
                |> Array.collect (fun r->r.Cells)
                |> Array.filter (fun c->c.CellType = Zander.CellType.Value)
                |> Array.map (fun c-> (c.Name, c.Value))
    let toList v =
        v
        |> Array.map (Array.map Array.toList)
        |> Array.map Array.toList
        |> Array.toList

    [<Fact>] 
    let ``Can parse complex example`` ()=
        let sections = [["";"H"];["D1";""];["D2";""];["";"D3"]]
                        |> List.map List.toArray
                        |> List.toArray
        let blockEx = BlockEx("_ \"H\" : header
        @D _ : data_rows+
        _ @D : data_rows2+
        ")
        let m = blockEx.Match(sections)
        m.Success |> should equal true
        m.Size |> should equal {Height=4;Width=2};
        getValues m |> should equal [| ("D","D1"); ("D","D2"); ("D","D3") |]
        m.WithName "data_rows" |> Array.length |> should equal 2
        m.WithName "data_rows2" |> Array.length |> should equal 1

    [<Fact>]
    let ``Can specify simple 2d block``() =
        let sections = [["";"H"];["D1";""];]
                        |> List.map List.toArray
                        |> List.toArray
        let blockEx = BlockEx("_ \"H\"
        @D _ 
        ")
        let m = blockEx.Match(sections)
        m.Success |> should equal true
        m.Size |> should equal {Height=2;Width=2};
        getValues m |> should equal [| ("D","D1"); |]

    [<Fact>]
    let ``Can specify simple 2d block with repeated row``() =
        let sections = [["";"H"];["D1";""];["D2";""];]
                        |> List.map List.toArray
                        |> List.toArray
        let blockEx = BlockEx("_ \"H\"
        @D _ :+
        ")
        let m = blockEx.Match(sections)
        m.Success |> should equal true
        m.Size |> should equal {Height=3;Width=2};
        getValues m |> should equal [| ("D","D1"); ("D","D2");|]

    [<Fact>]
    let ``Can split``() =
        let sections = [["";"H"];["D1";""];["D2";""];["";"H"];["D3";""];["D4";""];["";"H"];["D5";""];["D6";""];]
                        |> List.map List.toArray    
                        |> List.toArray
        let blockEx = BlockEx("_ \"H\"")
        let split = blockEx.Split(sections) |> toList
        //printf "%O" (split)
        split |> should equal [[];[["D1";""];["D2";""]];[["D3";""];["D4";""]];[["D5";""];["D6";""]]]

    [<Fact>]
    let ``Can split block with length``() =
        let sections = [["";"H"];["";"H2"];["D1";""];["D2";""];["";"H"];["";"H2"];["D3";""];["D4";""];["";"H"];["";"H2"];["D5";""];["D6";""];]
                        |> List.map List.toArray    
                        |> List.toArray
        let blockEx = BlockEx("_ \"H\"
        _ \"H2\"
        ")
        let split = blockEx.Split(sections) |> toList
        //printf "%O" (split)
        split |> should equal [[];[["D1";""];["D2";""]];[["D3";""];["D4";""]];[["D5";""];["D6";""]]]

    [<Fact>]
    let ``Can split blocks with different lengths``() =
        let sections = [["";"H"];["";"H"];["D1";""];["D2";""];["D2_";""];["";"H"];["D3";""];["D4";""];["";"H"];["D5";""];["D6";""];]
                        |> List.map List.toArray    
                        |> List.toArray
        let blockEx = BlockEx("_ \"H\" : +
        ")
        let split = blockEx.Split(sections)|> toList
        //printf "%O" (split)
        split |> should equal [[];[["D1";""];["D2";""];["D2_";""]];[["D3";""];["D4";""]];[["D5";""];["D6";""]]]

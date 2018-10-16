namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

module RowExTests = 
    let getValues (m:MatchRow)=
        m.Cells
        |> Array.filter (fun c->c.CellType = Zander.CellType.Value)
        |> Array.map (fun c-> (c.Name, c.Value))

    [<Fact>]
    let ``Can specify simple row``() =
        let sections = ["";"Header"]
                        |> List.toArray
        let rowEx = RowEx("_ @H")
        let m = rowEx.Match(sections)
        m.Success |> should equal true
        m.Length |> should equal 2
        getValues m |> should equal [| ("H","Header"); |]

    [<Fact>]
    let ``Can specify simple row repeated cells``() =
        let sections = ["";"D1";"D2"]
                        |> List.toArray
        let rowEx = RowEx("_ @D+")
        let m = rowEx.Match(sections)
        m.Success |> should equal true
        m.Length |> should equal 3
        getValues m |> should equal [| ("D","D1"); ("D","D2");|]


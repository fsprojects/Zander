namespace Tests
open Xunit
open FsUnit.Xunit
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

module NumberedMatchTests = 
    open Zander.Internal.Matches

    let zeroToThree = (fun m a -> 
                                match m, a with
                                    | '0', 0->Some "0"
                                    | '1', 1->Some "1"
                                    | '2', 2->Some "2"
                                    | '3', 3->Some "3"
                                    | _,_ -> None
                                )
    let someIsOk = Option.isSome

    [<Fact>] 
    let ``Match 1 on 1`` ()=
        let l = matches zeroToThree someIsOk [ (One, '1'); (One,'2'); (One,'3'); ]
                              [1;2;3] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "2");(Some "3")]

    [<Fact>] 
    let ``Match 1 on 1 where first is optional`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrOne, '1'); (One,'2'); (One,'3'); ]
                              [1;2;3] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "2");(Some "3")]

    [<Fact>] 
    let ``Match 1 on 1 where all are optional`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrOne, '1'); (ZeroOrOne,'2'); (ZeroOrOne,'3'); ]
                              [1;2;3] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "2");(Some "3")]

    [<Fact>] 
    let ``Match 1 on 1 where last is optional and missing`` ()=
        let l = matches zeroToThree someIsOk [ (One, '1'); (One,'2'); (ZeroOrOne,'3'); ]
                              [1;2] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "2")]

    [<Fact>] 
    let ``Match rest where first is optional and missing`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrOne, '1'); (One,'2'); (One,'3'); ]
                              [2;3] |> List.map MatchResult.value
        l |> should equal [(Some "2");(Some "3")]

    [<Fact>] 
    let ``Match many 1`` ()=
        let l = matches zeroToThree someIsOk [ (Many, '1'); ]
                              [1;1;1] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "1");(Some "1")]

    [<Fact>] 
    let ``Match many 1 and then 2`` ()=
        let l = matches zeroToThree someIsOk [ (Many, '1'); (One, '2') ]
                              [1;1;1;2] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "1");(Some "1"); (Some "2")]

    [<Fact>] 
    let ``Match zero or many 1`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrMany, '1'); ]
                              [1;1;1] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "1");(Some "1")]

    [<Fact>] 
    let ``Match zero or many 1 and then 2`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrMany, '1'); (One, '2')]
                              [1;1;1;2] |> List.map MatchResult.value
        l |> should equal [(Some "1");(Some "1");(Some "1"); (Some "2")]

    [<Fact>] 
    let ``Match zero or many 1 when empty`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrMany, '1'); ]
                              []
        Assert.Empty l

    [<Fact>] 
    let ``Match zero or one 1 when empty`` ()=
        let l = matches zeroToThree someIsOk [ (ZeroOrOne, '1'); ]
                              []
        Assert.Empty l
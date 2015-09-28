namespace Tests
open NUnit.Framework
open FsUnit
open Zander
open Zander.Internal
open TestHelpers

[<TestFixture>] 
module SplitTests = 
    open Zander.Internal.Matches

    [<Test>] 
    let ``Split on 1 where 1 starts`` ()=
        let l = [1;2;3;1;2;3] |> split ((=) 1)
        //printf "%O" l
        l |> should equal [[];[2;3];[2;3]]
    [<Test>] 
    let ``Split on 1`` ()=
        let l = [2;3;1;2;3;1;2;3] |> split ((=) 1)
        //printf "%O" l
        l |> should equal [[2;3];[2;3];[2;3]]

    [<Test>] 
    let ``Split list on 1 where 1 starts`` ()=
        let l = [1;2;3;1;2;3] |> split_list (fun l -> (List.head l |> ((=) 1), 1))
        //printf "%O" l
        l |> should equal [[];[2;3];[2;3]]
    [<Test>] 
    let ``Split list on 1`` ()=
        let l = [2;3;1;2;3;1;2;3] |> split_list (fun l -> (List.head l |> ((=) 1), 1))
        //printf "%O" l
        l |> should equal [[2;3];[2;3];[2;3]]

namespace Tests

open NUnit.Framework
open FsUnit
open BlockParser

[<TestFixture>] 
module RepeatRowBlockMatchTests=


    open Match

    let block_expression_with_repeat = 
        [
            Single, ([E; E; V; E; E; V; V]), "header"
            Repeat, ([V; E; E; E; E; E; E]), "subtitles"
            Single, ([E; C "th1"; E; C "th2"; E; C "th3"; E]), "header_row"
        ]

    [<Test>] 
    let ``Should match with no repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (block block_expression_with_repeat 0 a_block) |> should equal true

    [<Test>] 
    let ``Should match with repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["subtitle_1.2"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (block block_expression_with_repeat 0 a_block) |> should equal true

    [<Test>] 
    let ``Shouldnt match with repeated header`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                [""; ""; "Header2";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (block block_expression_with_repeat 0 a_block) |> should equal false


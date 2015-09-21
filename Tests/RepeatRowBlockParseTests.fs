namespace Tests

open NUnit.Framework
open FsUnit
open Zander.Internal

[<TestFixture>] 
module RepeatRowBlockParseTests=


    open Parse

    let block_expression_with_repeat = 
        [
            Single, ([Empty; Empty; Value ""; Empty; Empty; Value ""; Value ""]), "header"
            Repeat, ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "subtitles"
            Single, ([Empty; Const "th1"; Empty; Const "th2"; Empty; Const "th3"; Empty]), "header_row"
        ]

    [<Test>] 
    let ``Should match with no repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (rowsOf (block block_expression_with_repeat 0 a_block)) |> should equal [
                (["Header1";"Something else"; "Page:1"], "header")
                (["subtitle_1.1"],"subtitles")
                ([],"header_row")]

    [<Test>] 
    let ``Should match with repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["subtitle_1.2"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (rowsOf (block block_expression_with_repeat 0 a_block)) |> should equal [
            (["Header1";"Something else"; "Page:1"],"header")
            (["subtitle_1.1"],"subtitles")
            (["subtitle_1.2"],"subtitles")
            ([],"header_row")]


namespace Tests

open NUnit.Framework
open FsUnit
open Zander.Internal
open TestHelpers
[<TestFixture>] 
module RepeatRowBlockMatchTests=


    open Match

    let block_expression_with_repeat = 
        [
            One,    ([Empty; Empty; Value ""; Empty; Empty; Value ""; Value ""]), "header"
            Many,   ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "subtitles"
            One,    ([Empty; Const "th1"; Empty; Const "th2"; Empty; Const "th3"; Empty]), "header_row"
        ]

    [<Test>] 
    let ``Should match with no repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (match_s_block block_expression_with_repeat 0 a_block) |> should equal true

    [<Test>] 
    let ``Should match with repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["subtitle_1.2"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (match_s_block block_expression_with_repeat 0 a_block) |> should equal true

    [<Test>] 
    let ``Shouldnt match with repeated header`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                [""; ""; "Header2";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (match_s_block block_expression_with_repeat 0 a_block) |> should equal false


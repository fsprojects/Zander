namespace Tests

open FsUnit.Xunit
open FsUnit
open Xunit
open Zander.Internal

module RepeatRowBlockParseTests=


    open TestHelpers

    let block_expression_with_repeat = 
        [
            One,    ([Empty; Empty; Value ""; Empty; Empty; Value ""; Value ""]), "header"
            Many,   ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "subtitles"
            One,    ([Empty; Const "th1"; Empty; Const "th2"; Empty; Const "th3"; Empty]), "header_row"
        ]

    [<Fact>] 
    let ``Should match with no repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (rowsOf (sBlock block_expression_with_repeat a_block)) |> should equal [
                (["Header1";"Something else"; "Page:1"], "header")
                (["subtitle_1.1"],"subtitles")
                ([],"header_row")]

    [<Fact>] 
    let ``Should match repeated subtitles`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["subtitle_1.2"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (rowsOf (sBlock block_expression_with_repeat a_block)) |> should equal [
            (["Header1";"Something else"; "Page:1"],"header")
            (["subtitle_1.1"],"subtitles")
            (["subtitle_1.2"],"subtitles")
            ([],"header_row")]

    let block_expression_with_zero_or_many_subtitles = 
        [
            One,    ([Empty; Empty; Value ""; Empty; Empty; Value ""; Value ""]), "header"
            ZeroOrMany,   ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "subtitles"
            One,    ([Empty; Const "th1"; Empty; Const "th2"; Empty; Const "th3"; Empty]), "header_row"
        ]

    [<Fact>] 
    let ``Should match 0 or many subtitles : many`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["subtitle_1.1"; ""; "";"";"";""; ""]
                ["subtitle_1.2"; ""; "";"";"";""; ""]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (rowsOf (sBlock block_expression_with_zero_or_many_subtitles a_block)) |> should equal [
            (["Header1";"Something else"; "Page:1"],"header")
            (["subtitle_1.1"],"subtitles")
            (["subtitle_1.2"],"subtitles")
            ([],"header_row")]

    [<Fact>] 
    let ``Should match 0 or many subtitles : 0`` ()=
        let a_block = 
            [
                [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                ["";"th1"; ""; "th2";"";"th3";""]
            ]
        (rowsOf (sBlock block_expression_with_zero_or_many_subtitles a_block)) |> should equal [
            (["Header1";"Something else"; "Page:1"],"header")
            ([],"header_row")]

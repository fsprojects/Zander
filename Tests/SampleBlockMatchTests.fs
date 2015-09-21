namespace Tests

open NUnit.Framework
open FsUnit
open Zander.Internal

[<TestFixture>] 
module SampleBlockMatchTests=

    let block_expression = [
                    Single, ([Empty; Empty; Value ""; Empty; Empty; Value ""; Value ""]), "header"
                    Single, ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "title"
                    Repeat, ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "subtitles"
                    Single, ([Empty; Const "th1"; Empty; Const "th2"; Empty; Const "th3"; Empty]), "header_row"
                    Repeat, ([Empty; Value ""; Empty; Value ""; Empty; Value ""; Empty]), "data_rows"
                ]

    open Match

    [<Test>] 
    let ``Can recognize simple block expression`` ()=
        let a_block = [
                        [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                        ["title_1"; ""; "";"";"";""; ""]
                        ["subtitle_1.1"; ""; "";"";"";""; ""]
                        ["subtitle_1.2"; ""; "";"";"";""; ""]
                        ["subtitle_1.3"; ""; "";"";"";""; ""]
                        ["";"th1"; ""; "th2";"";"th3";""]
                        ["";"1.1.1"; ""; "1.1.2";"";"1.1.3";""]
                        ["";"1.2.1"; ""; "1.2.2";"";"1.2.3";""]
        ]
        (block block_expression 0 a_block) |> should equal true


    [<Test>] 
    let ``Shouldnt recognize with wrong header`` ()=
        let a_block = [
                        [""; ""; "Header1";"XX";"";"Something else"; "Page:1"]
                        ["title_1"; ""; "";"";"";""; ""]
                        ["subtitle_1.1"; ""; "";"";"";""; ""]
                        ["subtitle_1.2"; ""; "";"";"";""; ""]
                        ["subtitle_1.3"; ""; "";"";"";""; ""]
                        ["";"th1"; ""; "th2";"";"th3";""]
                        ["";"1.1.1"; ""; "1.1.2";"";"1.1.3";""]
                        ["";"1.2.1"; ""; "1.2.2";"";"1.2.3";""]
        ]
        (block block_expression 0 a_block) |> should equal false

    [<Test>] 
    let ``Shouldnt recognize with wrong title row`` ()=
        let a_block = [
                        [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                        ["title_1"; ""; "XX";"";"";""; ""]
                        ["subtitle_1.1"; ""; "";"";"";""; ""]
                        ["subtitle_1.2"; ""; "";"";"";""; ""]
                        ["subtitle_1.3"; ""; "";"";"";""; ""]
                        ["";"th1"; ""; "th2";"";"th3";""]
                        ["";"1.1.1"; ""; "1.1.2";"";"1.1.3";""]
                        ["";"1.2.1"; ""; "1.2.2";"";"1.2.3";""]
        ]
        (block block_expression 0 a_block) |> should equal false

    [<Test>] 
    let ``Should not recognize other block`` ()=
           (block block_expression 0 [[""]]) |> should equal false



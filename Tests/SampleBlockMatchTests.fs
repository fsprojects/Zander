namespace Tests

open Xunit
open FsUnit.Xunit
open FsUnit
open Zander.Internal

module SampleBlockMatchTests=
    open TestHelpers

    let block_expression =
         [
            One,    ([Empty; Empty; Value ""; Empty; Empty; Value ""; Value ""]), "header"
            One,    ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "title"
            Many,   ([Value ""; Empty; Empty; Empty; Empty; Empty; Empty]), "subtitles"
            One,    ([Empty; Const "th1"; Empty; Const "th2"; Empty; Const "th3"; Empty]), "header_row"
            Many,   ([Empty; Value ""; Empty; Value ""; Empty; Value ""; Empty]), "data_rows"
         ] 
         |> mapToBlockSingleColumns

    [<Fact>] 
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
        parseAndMatchBlock block_expression a_block |> should equal true


    [<Fact>] 
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
        parseAndMatchBlock block_expression a_block |> should equal false

    [<Fact>] 
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
        parseAndMatchBlock block_expression a_block |> should equal false

    [<Fact>] 
    let ``Should not recognize other block`` ()=
        parseAndMatchBlock block_expression [[""]] |> should equal false



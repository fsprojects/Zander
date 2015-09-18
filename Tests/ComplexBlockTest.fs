namespace Tests
open NUnit.Framework
open FsUnit
open Zander.Internal

[<TestFixture>] 
module ComplexBlockTest = 


    let first_section = [
                        [""; ""; "Header1";"";"";"Something else"; "Page:1"]
                        ["title_1"; ""; "";"";"";""; ""]
                        ["subtitle_1.1"; ""; "";"";"";""; ""]
                        ["subtitle_1.2"; ""; "";"";"";""; ""]
                        ["subtitle_1.3"; ""; "";"";"";""; ""]
                        ["";"th1"; ""; "th2";"";"th3";""]
                        ["";"1.1.1"; ""; "1.1.2";"";"1.1.3";""]
                        ["";"1.2.1"; ""; "1.2.2";"";"1.2.3";""]
    ]

    let specification = [
                        Single, ([E; E; V; E; E; V; V]), "header"
                        Single, ([V; E; E; E; E; E; E]), "title"
                        Repeat, ([V; E; E; E; E; E; E]), "subtitles"
                        Single, ([E; C "th1"; E; C "th2"; E; C "th3"; E]), "header_row"
                        Repeat, ([E; V; E; V; E; V; E]), "data_rows"
                    ]    

    let expected =  [(["Header1";"Something else"; "Page:1"], "header")
                     (["title_1"], "title")
                     (["subtitle_1.1"],"subtitles")
                     (["subtitle_1.2"],"subtitles")
                     (["subtitle_1.3"],"subtitles")
                     ([],"header_row")
                     (["1.1.1"; "1.1.2";"1.1.3"],"data_rows")
                     (["1.2.1"; "1.2.2";"1.2.3"],"data_rows")
                     ]
    [<Test>] 
    let ``Can parse complex example`` ()=
           (Parse.rowsOf (Parse.block specification 0 first_section)) |> should equal expected
           //(spec first_section) |> Seq.head |> should equal expected


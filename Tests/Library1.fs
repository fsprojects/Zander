namespace Tests
open NUnit.Framework
open FsUnit
open BlockParser

[<TestFixture>] 
module Class1 = 
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

    open Match

    let parseBlocks spec v =
        []
    let blockParse expr index blocks =
        [],1

    type Builder()=
        let mutable array : (NumberOf* BlockType list * string ) list list= [];

        member this.Block(x : (NumberOf* BlockType list * string) list ) = 
            array <- array |> List.append [ x ]
            this

        member this.Parse(blocks : string list list) =
            let rec parse index =
                let next =  array |> List.find (fun sp-> (Match.block sp index blocks ) )
                let (parsed, nextIndex) = blockParse next index blocks
                [ parsed ] @ (parse nextIndex) 
            parse 0


    let builder = new Builder()
    

    let spec input= 
            builder
                .Block([
                        Single, ([E; E; V; E; E; V; V]), "header"
                        Single, ([V; E; E; E; E; E; E]), "title"
                        Repeat, ([V; E; E; E; E; E; E]), "subtitles"
                        Single, ([E; C "th1"; E; C "th2"; E; C "th3"; E]), "header_row"
                        Repeat, ([E; V; E; V; E; V; E]), "data_rows"
                    ]).Parse( input )

    let expected = dict (["header", [["Header1";"Something else"; "Page:1"]]
                          "title", [["title_1"]]
                          "subtitles", [["subtitle_1.1"]; ["subtitle_1.2"]; ["subtitle_1.3"]]
                          "header_row", []
                          "data_rows", [
                                ["1.1.1"; "1.1.2";"1.1.3"]
                                ["1.2.1"; "1.2.2";"1.2.3"]
                          ]
                         ])
(*
    [<Test>] 
    let ``Can parse simple section`` ()=
           (spec first_section) |> Seq.head |> should equal expected
           *)
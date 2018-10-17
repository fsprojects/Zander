namespace Tests
open System
open Xunit
open FsUnit
open FsUnit.Xunit
open Zander.Internal
open TestHelpers
module StringTests = 
    open Zander.Internal.StringAndPosition

    [<Fact>] 
    let ``Regex match starting with`` ()=
        regexMatchI "^abc" {input="abcxyz";position=0} |> Option.map snd |> should equal (Some 3)
        regexMatchI "^abc" {input="???abcxyz";position=3} |> Option.map snd |> should equal (Some 3)


    [<Fact>] 
    let ``Regex match ending with`` ()=
        regexMatchI "^abc" {input="xyzabc";position=0} |> should equal (None)

    [<Fact>] 
    let ``match quoted constant`` ()=
        let input = "\"Test\""
        match {input=input;position=0} with | LooksLikeConstant (Some (c, l)) -> Some(c,l) ; | _-> None
            |> should equal (Some ("Test",6))
        input.Length |> should equal 6

    [<Fact>] 
    let ``match quoted constant among other`` ()=
        let input ="abc \"Test\" ert"
        match {input=input;position=4} with | LooksLikeConstant (Some (c, l)) -> Some(c,l) ; | _-> None
            |> should equal (Some ("Test",6))
        input.Substring(4,6) |> should equal "\"Test\""

    [<Fact>] 
    let ``should not match quoted constant if it's not quoted`` ()=
         match {input="abc Test\" ert";position=4} with | LooksLikeConstant (Some (c, l)) -> Some(c,l) ; | _-> None
            |> should equal None
     





    (*
       let to_column (v:string*int) : (string option*int)  =
        match v with
            | LooksLikeConstant (Some (c, l)) -> (Some "C"), l 
            | RegexMatch "_"  ([g], l) -> (Some "E"), l
            | RegexMatch @"\@\w*" ([value], l) -> Some( value.Value.Substring(1) ) , l
            | RegexMatch @"\s+" ([g], l) -> None, l
            | _ -> failwithf "! '%s' %i" ((fst v).Substring(snd v)) (snd v)
    *)
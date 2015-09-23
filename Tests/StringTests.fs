namespace Tests
open System
open NUnit.Framework
open FsUnit
open Zander.Internal
open TestHelpers

module StringTests = 
    open Zander.Internal.String

    [<Test>] 
    let ``Regex match starting with`` ()=
        regex_match_i "^abc" {input="abcxyz";position=0} |> Option.map snd |> should equal (Some 3)
        regex_match_i "^abc" {input="???abcxyz";position=3} |> Option.map snd |> should equal (Some 3)


    [<Test>] 
    let ``Regex match ending with`` ()=
        regex_match_i "^abc" {input="xyzabc";position=0} |> should equal (None)


    (*
     match (" @Test",0) with | RegexMatch "^\@[A-Z]\w*" ([g], l) -> Some (g, l) ; | _ -> None;;
     match (" @Test",1) with | RegexMatch "\@[A-Z]\w*" ([g], l) -> Some (g, l) ; | _ -> None;;
     match ("@Test     ",0) with | RegexMatch "^\@[A-Z]\w*" ([g], l) -> Some (g, l) ; | _ -> None;;
    *)

    [<Test>] 
    let ``match looks like constant`` ()=
         match {input="\"Test\"";position=0} with | Api.LooksLikeConstant (Some (c, l)) -> Some(c,l) ; | _-> None
            |> should equal (Some ("Test",6))

         match {input="abc \"Test\" ert";position=4} with | Api.LooksLikeConstant (Some (c, l)) -> Some(c,l) ; | _-> None
            |> should equal (Some ("Test",6))
     

         match {input="abc Test\" ert";position=4} with | Api.LooksLikeConstant (Some (c, l)) -> Some(c,l) ; | _-> None
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
namespace Zander.Internal
open System
open Zander.Internal.Option

module Matches= 
    let rec mapWhile (map: ('T -> 'V)) ( predicate )= function 
    | [] -> []
    | (source: 'T list)->
        let h = map (List.head source)
        match predicate h with
            | true -> h :: mapWhile map predicate (List.tail source)
            | false -> []
    type MatchResult<'t> =
        | MatchOk of 't
        | MatchEmptyList
        | MatchFailure

    module MatchResult=
        let value m=
            match m with
                | MatchOk v->v
                | _ -> failwith "Not an ok match!"

    let sliceOrEmpty i = function
        | [] -> []
        | list-> 
            if  List.length list > i then
                list.[i ..]
            else
                []

    let matches 
            (matcher:'m->'a->'v) 
            (predicate: 'v->bool)
            (expr: (NumberOf*'m) list) 
            (row: 'a list) : (MatchResult<'v>) list =

        let rec matchMany = function
            | [],[] -> []
            | recognizers,input -> 

                let h_rec = recognizers.[0]
                let match_h_rec = (matcher (snd h_rec))
                match (fst h_rec) with
                    | ZeroOrMany -> 
                        let matchedList = mapWhile match_h_rec predicate input |> List.map MatchOk 
                        matchedList @ matchMany ( (sliceOrEmpty 1 recognizers), (sliceOrEmpty matchedList.Length input) )
                    | One -> 
                        if List.isEmpty input then
                            [MatchEmptyList]
                        else
                            let h_input = List.head input
                            let head = match_h_rec h_input
                            if 1 >= input.Length then
                                [MatchOk(head)]
                            else
                                MatchOk(head) ::  matchMany ((sliceOrEmpty 1 recognizers), (sliceOrEmpty 1 input) )
                    | Many -> 
                        matchMany (([ (One,snd h_rec) ; (ZeroOrMany,snd h_rec)] @ (sliceOrEmpty 1 recognizers)), input)
        matchMany (expr, row)

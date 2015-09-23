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

    let matches 
            (matcher:'m->'a->'v) 
            (predicate: 'v->bool)
            (expr: (NumberOf*'m) list) 
            (row: 'a list) : (MatchResult<'v>) list =

        let rec matchMany
            (recognizers: (NumberOf*'m) list) 
            (input:'a list) : MatchResult<'v> list= 

            let h_rec = recognizers.[0]
            let match_h_rec = (matcher (snd h_rec))
            match (fst h_rec) with
                | ZeroOrMany -> mapWhile match_h_rec predicate input |> List.map MatchOk
                | One -> 
                    if List.isEmpty input then
                        [MatchEmptyList]
                    else
                        let h_input = List.head input
                        let head = match_h_rec h_input
                        if 1 >= input.Length then
                            [MatchOk(head)]
                        else
                            MatchOk(head) :: ( matchMany (recognizers.[1..]) input.[1..] )
                | Many -> 
                    matchMany ([ (One,snd h_rec) ; (ZeroOrMany,snd h_rec)] @ recognizers.[1..]) input
        matchMany expr row

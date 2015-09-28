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
    type MatchResult<'t,'a> =
        | MatchOk of 't
        | MatchEmptyList
        | MatcherMissing of 'a
        | MatchFailure

    module MatchResult=
        let isOk m =
            match m with
                | MatchOk _-> true
                | _ -> false
        let value m=
            match m with
                | MatchOk v->v
                | _ -> failwith "Not an ok match!"

    let sliceOrEmpty (from: int option) (to' : int option)= function
        | [] -> []
        | (list : _ list)-> 
            let none_if_out_of_bounds v=
                opt{ 
                    let! v' = v
                    return! if List.length list > v' && v'>=0 then Some v' else None
                }
            let to'' = none_if_out_of_bounds to'
            let from'' =none_if_out_of_bounds from
            if  Option.isSome from'' || Option.isSome to'' then
                list.GetSlice (from'', to'')
            else
                []

    let rec split (matcher: 'v -> bool) = function
            | [] -> []
            | (input: 'v list) ->
                let maybeIndex = input |> List.tryFindIndex matcher
                match maybeIndex with
                       | None -> [ input ]
                       | Some idx -> (sliceOrEmpty None (Some (idx-1)) input) 
                                        :: split matcher (sliceOrEmpty (Some(idx+1)) (Some (List.length input-1)) input)

    let rec split_list (matcher: 'v list -> (bool*int)) = function
            | [] -> []
            | (input: 'v list)->

                let rec tryFindIndex matcher (position:int)= function
                    | [] -> None
                    | input ->
                        let m = matcher input
                        if fst m then
                            Some (position, snd m)
                        else
                            match input with
                            | [] -> None
                            | _::tail -> tryFindIndex matcher (position+1) tail 


                let maybeIndex = input |> tryFindIndex matcher 0
                match maybeIndex with
                       | None -> [ input ]
                       | Some (idx,len) -> (sliceOrEmpty None (Some (idx-1)) input) 
                                        :: split_list matcher (sliceOrEmpty (Some(idx+len)) (Some (List.length input-1)) input)

    let matches 
            (matcher:'m->'a->'v) 
            (predicate: 'v->bool)
            (expr: (NumberOf*'m) list) 
            (row: 'a list) : (MatchResult<'v,'a>) list =

        let rec matchMany = function
            | [],[] -> []
            | [], v -> v |> List.map MatcherMissing 
            | (One,_)::_, [] -> [MatchEmptyList]
            | recognizers,input -> 
                let matchTail r_num num=
                    matchMany ((sliceOrEmpty (Some r_num) None recognizers), (sliceOrEmpty (Some num) None input) )

                let h_rec = recognizers.[0]
                let match_h_rec = (matcher (snd h_rec))
                match (fst h_rec) with
                    | ZeroOrMany -> 
                        let matchedList = mapWhile match_h_rec predicate input |> List.map MatchOk 
                        matchedList @ matchTail 1 matchedList.Length 
                    | One -> 
                        let h_input = List.head input
                        let head = match_h_rec h_input
                        MatchOk(head) ::  matchTail 1 1
                    | ZeroOrOne ->
                        if List.isEmpty input then
                            matchTail 1 0
                        else
                            let h_input = List.head input
                            let head = match_h_rec h_input
                            if predicate head then
                                MatchOk(head) ::  matchTail 1 1
                            else
                                matchTail 1 0
                    | Many -> 
                        matchMany (([ (One,snd h_rec) ; (ZeroOrMany,snd h_rec)] @ (sliceOrEmpty (Some 1) None recognizers)), input)
        matchMany (expr, row)

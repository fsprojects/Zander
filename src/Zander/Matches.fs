namespace Zander.Internal
open System
type MatchError<'a>=
    | MatchEmptyList
    | MatcherMissing of 'a
    | MatchFailure
module Matches= 

    let sliceOrEmpty (from: int option) (to' : int option)= function
        | [] -> []
        | (list : _ list)-> 
            let notOutOfBounds idx = List.length list > idx && idx>=0
            let noneIfOutOfBounds v= v |> Option.bind (fun v'-> if notOutOfBounds v' then Some v' else None)
            let to'' = noneIfOutOfBounds to'
            let from'' =noneIfOutOfBounds from
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

    let rec splitList (matcher: 'v list -> (bool*int)) = function
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
                                        :: splitList matcher (sliceOrEmpty (Some(idx+len)) (Some (List.length input-1)) input)

    let matches 
            (matcher:'m->'a->'v) 
            (predicate: 'v->bool)
            (expr: (NumberOf*'m) list) 
            (row: 'a list) : (Result<'v,MatchError<'a>>) list =

        let rec matchMany = function
            | [],[] -> []
            | [], v -> v |> List.map (fun a->Error <| MatcherMissing a) 
            | (One,_)::_, [] -> [Error MatchEmptyList]
            | recognizers,input -> 
                let matchTail r_num num=
                    matchMany ((sliceOrEmpty (Some r_num) None recognizers), (sliceOrEmpty (Some num) None input) )

                let h_rec = recognizers.[0]
                let match_h_rec = (matcher (snd h_rec))
                match (fst h_rec) with
                    | ZeroOrMany -> 
                        let matchedList = Seq.map match_h_rec input
                                          |> Seq.takeWhile predicate |> Seq.map Ok |> Seq.toList
                        matchedList @ matchTail 1 matchedList.Length
                    | One -> 
                        let h_input = List.head input
                        let head = match_h_rec h_input
                        Ok head ::  matchTail 1 1
                    | ZeroOrOne ->
                        if List.isEmpty input then
                            matchTail 1 0
                        else
                            let h_input = List.head input
                            let head = match_h_rec h_input
                            if predicate head then
                                Ok head ::  matchTail 1 1
                            else
                                matchTail 1 0
                    | Many -> 
                        matchMany (([ (One,snd h_rec) ; (ZeroOrMany,snd h_rec)] @ (sliceOrEmpty (Some 1) None recognizers)), input)
        matchMany (expr, row)

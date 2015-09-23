module TestHelpers
open System.Collections.Generic
open Zander.Internal
let kv key value=
    new KeyValuePair<_,_>(key, value)
let map_to_single c=
    One,c
let map_to_block_single_columns block_expression = 
    block_expression
           |> List.map (fun (count, row, title)->(count, row|>List.map map_to_single, title))

let s_expression expr=
    Parse.expression (List.map map_to_single expr)
let s_block expr=
    Parse.block (map_to_block_single_columns expr)

let match_s_expression expr=
    Match.expression (List.map map_to_single expr)

let match_s_block expr=
    Match.block (map_to_block_single_columns expr)

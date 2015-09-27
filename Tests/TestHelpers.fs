module TestHelpers
open System.Collections.Generic
open Zander.Internal
let kv key value=
    new KeyValuePair<_,_>(key, value)
let map_to_single c=
    One,c
let map_to_block_single_columns block_expression = 
    block_expression
           |> List.map (fun (count,row,title)->{num=count; recognizer= (row|>List.map map_to_single); name= title})

let s_expression expr=
    Parse.expression (List.map map_to_single expr)
let s_block expr=
    Parse.block (map_to_block_single_columns expr)

let match_s_expression expr=
    Parse.expression (List.map map_to_single expr) >> Match.expression
let parse_and_match_expression expr=
    Parse.expression expr >> Match.expression

let match_s_block expr index=
    Parse.block (map_to_block_single_columns expr) index >> Match.block
let parse_and_match_block expr index=
    Parse.block expr index >> Match.block

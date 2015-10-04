module TestHelpers
open System.Collections.Generic
open Zander
open Zander.Internal
let kv key value=
    new KeyValuePair<_,_>(key, value)
let map_to_single c=
    One,c
let map_to_block_single_columns block_expression = 
    block_expression
           |> List.map (fun (count,row,title)->(count, {recognizer= (row|>List.map map_to_single); name= title}))
let opts=ParseOptions.Default

let s_expression expr=
    Parse.expression (List.map map_to_single expr) opts
let s_block expr=
    Parse.block (map_to_block_single_columns expr) opts

let match_s_expression expr=
    Parse.expression (List.map map_to_single expr) opts>> Match.expression
let match_s_expression_opt expr opt=
    Parse.expression (List.map map_to_single expr) opt>> Match.expression
let parse_and_match_expression expr=
    Parse.expression expr opts>> Match.expression

let match_s_block expr =
    Parse.block (map_to_block_single_columns expr) opts >> Match.block
let parse_and_match_block expr =
    Parse.block expr opts  >> Match.block

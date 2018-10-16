module TestHelpers
open System.Collections.Generic
open Zander
open Zander.Internal
let kv key value=
    new KeyValuePair<_,_>(key, value)
let mapToSingle c=
    One,c
let mapToBlockSingleColumns blockExpression = 
    blockExpression
           |> List.map (fun (count,row,title)->(count, {recognizer= (row|>List.map mapToSingle); name= title}))
let opts=ParseOptions.Default

let sExpression expr=
    Parse.expression (List.map mapToSingle expr) opts
let sBlock expr=
    Parse.block (mapToBlockSingleColumns expr) opts

let matchSExpression expr=
    Parse.expression (List.map mapToSingle expr) opts>> Match.expression
let matchSExpressionOpt expr opt=
    Parse.expression (List.map mapToSingle expr) opt>> Match.expression
let parseAndMatchExpression expr=
    Parse.expression expr opts>> Match.expression

let matchSBlock expr =
    Parse.block (mapToBlockSingleColumns expr) opts >> Match.block
let parseAndMatchBlock expr =
    Parse.block expr opts  >> Match.block

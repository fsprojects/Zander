module TestHelpers
open System.Collections.Generic
open Zander
open Zander.Internal
let kv key value=KeyValuePair<_,_>(key, value)
let mapToSingle c=
    One,c
let mapToBlockSingleColumns blockExpression = 
    blockExpression
           |> List.map (fun (count,row,title)->(count, {recognizer= (row|>List.map mapToSingle); name= title}))
let opts=ParseOptions.Default
/// Returns the values of 'Value' where the match is ok
let rowsOf v = 
    let valuesOf v' =
        v'
        |> List.choose Result.tryValue
        |> List.choose Token.tryValue

    v |> List.map (fun (row,name)-> (valuesOf row) , name)

let sExpression expr=
    Row.parse (List.map mapToSingle expr) opts
let sBlock expr=
    Block.parse (mapToBlockSingleColumns expr) opts

let valuesOfExpression v a= 
    sExpression v a 
        |> List.map Result.value
        |> List.choose Token.tryValue

let matchSExpression expr=
    Row.parse (List.map mapToSingle expr) opts>> Match.expression
let matchSExpressionOpt expr opt=
    Row.parse (List.map mapToSingle expr) opt>> Match.expression
let parseAndMatchExpression expr=
    Row.parse expr opts>> Match.expression

let matchSBlock expr =
    Block.parse (mapToBlockSingleColumns expr) opts >> Match.block
let parseAndMatchBlock expr =
    Block.parse expr opts  >> Match.block

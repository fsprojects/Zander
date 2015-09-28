namespace Zander.Internal
open System
open System.Text.RegularExpressions
type InputAndPosition<'t> = { input:'t; position:int }

type StringAndPosition = InputAndPosition<string>

type StringAndLength = (string*int)

module internal String = 

    let sub_i (input:StringAndPosition) =
        let {input=s; position=i} = input
        s.Substring(i)

    let get_input (v:StringAndPosition) =
        v.input

    let get_position (v:StringAndPosition) =
        v.position

    let emptyPosition s=
        { input =s; position=0 }

    let regex_match_i pattern input=
        let r = new Regex(pattern)
        let m = r.Match ( sub_i input )
        if m.Success then
            Some ([for x in m.Groups -> x], m.Length)
        else 
            None

    let (|RegexMatch|_|) pattern (input:StringAndPosition) =
        let r = new Regex(pattern)
        if (get_input input) = null then 
            None
        else 
            regex_match_i pattern input

    let s_incr add (input:StringAndPosition) : StringAndPosition =
        {input=get_input input;position= add+(get_position input)}


namespace Zander.Internal
open System
open System.Text.RegularExpressions
type InputAndPosition<'t> = { input:'t; position:int }

type StringAndPosition = InputAndPosition<string>

type StringAndLength = (string*int)

module internal String = 

    let sub (input:StringAndPosition) =
        let {input=s; position=i} = input
        s.Substring(i)

    let getInput (v:StringAndPosition) =
        v.input

    let getPosition (v:StringAndPosition) =
        v.position

    let getLength (v:StringAndPosition) =
        v.input.Length

    let emptyPosition s=
        { input =s; position=0 }

    let toString o=
        o.ToString()
    let trim chars (v:string)=
        v.Trim(chars|>List.toArray)
    let regexMatchI pattern input=
        let r = new Regex(pattern)
        let m = r.Match ( sub input )
        if m.Success then
            Some ([for x in m.Groups -> x], m.Length)
        else 
            None

    let (|RegexMatch|_|) pattern (input:StringAndPosition) =
        if (getInput input) = null then 
            None
        else 
            regexMatchI pattern input

    let sIncr add (input:StringAndPosition) : StringAndPosition =
        {input=getInput input;position= add+(getPosition input)}


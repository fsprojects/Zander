namespace Zander.Internal
open System.Runtime.CompilerServices
[<assembly: InternalsVisibleTo("Zander.Tests")>]
do ()
open System
open System.Text.RegularExpressions
type InputAndPosition<'t> = { input:'t; position:int }

type StringAndPosition = InputAndPosition<string>

type StringAndLength = (string*int)

module internal StringAndPosition =

    let sub {input=(s:string); position=i} = s.Substring(i)

    let input (v:StringAndPosition) = v.input

    let position (v:StringAndPosition) = v.position

    let length (v:StringAndPosition) = v.input.Length

    let firstPosition s= { input =s; position=0 }

    let regexMatchI pattern input=
        let m = Regex.Match ( sub input, pattern )
        if m.Success then
            Some ([for x in m.Groups -> x], m.Length)
        else 
            None

    let (|RegexMatch|_|) pattern (value:StringAndPosition) =
        if (input value) = null then
            None
        else 
            regexMatchI pattern value

    let sIncr add (value:StringAndPosition) : StringAndPosition =
        {input=input value;position= add+(position value)}


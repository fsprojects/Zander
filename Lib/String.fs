namespace Zander.Internal
open System
open System.Text.RegularExpressions

module String = 
    let sub_i (input:string*int) =
        let (s,i) = input
        s.Substring(i)

    let regex_match_i pattern input=
        let r = new Regex(pattern)
        let m = r.Match ( sub_i input )
        if m.Success then
            Some ([for x in m.Groups -> x], m.Length)
        else 
            None

    let (|RegexMatch|_|) pattern input =
        let r = new Regex(pattern)
        if (fst input) = null then 
            None
        else 
            regex_match_i pattern input

    let s_incr add (input:string*int) =
        (fst input, add+(snd input))
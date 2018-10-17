namespace Zander.Internal

module IndexOfNonEscapedQuote=
    open Zander.Internal.StringAndPosition
    open Zander.Internal.Option
    /// internal state to identify unescaped double quotes
    type NonEscapedQuoteState=
        | Start
        | Escape of int
        | Something of int
        | EndOfString
        | UnEscapedQuote of int
    /// internal code to identify unescaped double quotes
    let rec transitionNonEscapedQuote (input:string) s =
        let somethingTransition next=
            match input.[next] with
            | '\\' -> 
                Escape next |> transitionNonEscapedQuote input
            | '"' -> 
                UnEscapedQuote next
            | _ ->
                Something next |> transitionNonEscapedQuote input

        match s with
        | Start ->
            let next =0
            somethingTransition next
        | Escape l when input.Length>l+1->
            Something (l+1)
            |> transitionNonEscapedQuote input
        | Escape l ->
            EndOfString
        | Something l when input.Length>l+1-> 
            let next =l+1
            somethingTransition next 
        | Something l ->
            EndOfString
        // end states:
        | EndOfString -> EndOfString
        | UnEscapedQuote l -> UnEscapedQuote l

    let indexOfFirstNonEscapedQuote (input :StringAndPosition)= 
            let index =
                match transitionNonEscapedQuote (sub input) Start with
                | UnEscapedQuote l-> Some l
                | EndOfString -> None
                | _ -> failwith "this should not happen"
            index |> Option.map (fun i->input.position+i)

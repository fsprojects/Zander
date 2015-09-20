namespace Zander.Internal
open System

module Option = 
    type OptionBuilder() =
        member x.Bind(v,f) = Option.bind f v
        member x.Return v = Some v
        member x.ReturnFrom o = o

    let opt = OptionBuilder()
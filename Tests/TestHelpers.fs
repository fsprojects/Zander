module TestHelpers
open System.Collections.Generic

let kv key value=
    new KeyValuePair<_,_>(key, value)
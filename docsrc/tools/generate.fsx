// --------------------------------------------------------------------------------------
// Builds the documentation from `.fsx` and `.md` files in the 'docs/content' directory
// (the generated documentation is stored in the 'docs/output' directory)
// --------------------------------------------------------------------------------------

// Web site location for the generated documentation
#if TESTING
let website = __SOURCE_DIRECTORY__ + "../output"
#else
let website = "/FSharp.Formatting"
#endif

let githubLink = "http://github.com/fsprojects/Zander"

// Specify more information about your project
let info =
  [ "project-name", "Zander"
    "project-author", "Oskar Gewalli"
    "project-summary", "Small library to extract information out of matrix blocks (read matrix data from excel, csv)"
    "project-github", githubLink
    "project-nuget", "http://nuget.org/packages/Zander" ]

let referenceBinaries =
  [ "Zander.dll"; ]

// --------------------------------------------------------------------------------------
// For typical project, no changes are needed below
// --------------------------------------------------------------------------------------


#I "../../packages/FAKE/tools/"
#r "NuGet.Core.dll"
#r "FakeLib.dll"
open Fake
open System.IO
open Fake
open Fake.FileHelper

#load "../../packages/Formatting/FSharp.Formatting/FSharp.Formatting.fsx"

open FSharp.Literate
open FSharp.MetadataFormat
open FSharp.Formatting.Razor

// When called from 'build.fsx', use the public project URL as <root>
// otherwise, use the current 'output' directory.
#if RELEASE
let root = website
#else
let root = "file://" + (__SOURCE_DIRECTORY__ @@ "../output")
#endif

System.IO.Directory.SetCurrentDirectory (__SOURCE_DIRECTORY__)

// Paths with template/source/output locations
let bin        = "../../bin"
let content    = "../content"
let output     = "../output"
let files      = "../files"
let templates  = "."
let formatting = "../../misc/"
let docTemplate = formatting @@ "templates/docpage.cshtml"
let docTemplateSbS = templates @@ "docpage-sidebyside.cshtml"

// Where to look for *.csproj templates (in this order)
let layoutRootsAll = new System.Collections.Generic.Dictionary<string, string list>()
layoutRootsAll.Add("en",[ templates; formatting @@ "templates"
                          formatting @@ "templates/reference" ])
subDirectories (directoryInfo templates)
|> Seq.iter (fun d ->
                let name = d.Name
                if name.Length = 2 || name.Length = 3 then
                    layoutRootsAll.Add(
                            name, [templates @@ name
                                   formatting @@ "templates"
                                   formatting @@ "templates/reference" ]))

//let fsiEvaluator = lazy (Some (FsiEvaluator() :> IFsiEvaluator))

// Copy static files and CSS + JS from F# Formatting
let copyFiles () =
  CopyRecursive files output true |> Log "Copying file: "
  ensureDirectory (output @@ "content")
  //CopyRecursive (formatting @@ "styles") (output @@ "content") true
  //  |> Log "Copying styles and scripts: "

let binaries =
    referenceBinaries
    |> List.ofSeq
    |> List.map (fun b -> bin @@ b)

let libDirs = [bin]

// Build API reference from XML comments
let buildReference () =
  CleanDir (output @@ "reference")
  RazorMetadataFormat.Generate
    ( binaries, output @@ "reference", layoutRootsAll.["en"],
      parameters = ("root", root)::info,
      sourceRepo = githubLink @@ "tree/master",
      sourceFolder = __SOURCE_DIRECTORY__ @@ ".." @@ "..",
      publicOnly = true, libDirs = libDirs)

// Build documentation from `fsx` and `md` files in `docs/content`
let buildDocumentation () =
  let subdirs =
    [ content @@ "sidebyside", docTemplateSbS
      content, docTemplate; ]
  for dir, template in subdirs do
    let sub = "." // Everything goes into the same output directory here
    let langSpecificPath(lang, path:string) =
        path.Split([|'/'; '\\'|], System.StringSplitOptions.RemoveEmptyEntries)
        |> Array.exists(fun i -> i = lang)
    let layoutRoots =
        let key = layoutRootsAll.Keys |> Seq.tryFind (fun i -> langSpecificPath(i, dir))
        match key with
        | Some lang -> layoutRootsAll.[lang]
        | None -> layoutRootsAll.["en"] // "en" is the default language
    RazorLiterate.ProcessDirectory
      ( dir, template, output @@ sub, replacements = ("root", root)::info,
        layoutRoots = layoutRoots,
        generateAnchors = true,
        processRecursive = false,
        includeSource = true
         // Only needed for 'side-by-side' pages, but does not hurt others
      )
       //, ?fsiEvaluator = fsiEvaluator.Value ) // Currently we don't need it but it's a good stress test to have it here.

let watch () =
  printfn "Starting watching by initial building..."
  let rebuildDocs () =
    CleanDir output // Just in case the template changed (buildDocumentation is caching internally, maybe we should remove that)
    copyFiles()
    buildReference()
    buildDocumentation()
  rebuildDocs()
  printfn "Watching for changes..."

  let full s = Path.GetFullPath s
  let queue = new System.Collections.Concurrent.ConcurrentQueue<_>()
  let processTask () =
    async {
      let! tok = Async.CancellationToken
      while not tok.IsCancellationRequested do
        try
          if queue.IsEmpty then
            do! Async.Sleep 1000
          else
            let data = ref []
            let hasData = ref true
            while !hasData do
              match queue.TryDequeue() with
              | true, d ->
                data := d :: !data
              | _ ->
                hasData := false

            printfn "Detected changes (%A). Invalidate cache and rebuild." !data
            FSharp.Formatting.Razor.RazorEngineCache.InvalidateCache (!data |> Seq.map (fun change -> change.FullPath))
            rebuildDocs()
            printfn "Documentation generation finished."
        with e ->
          printfn "Documentation generation failed: %O" e
    }

  use watcher =
    !! (full content + "/*.*")
    ++ (full templates + "/*.*")
    ++ (full files + "/*.*")
    ++ (full formatting + "templates/*.*")
    |> WatchChanges (fun changes ->
      changes |> Seq.iter queue.Enqueue)
  use source = new System.Threading.CancellationTokenSource()
  Async.Start(processTask (), source.Token)
  printfn "Press enter to exit watching..."
  System.Console.ReadLine() |> ignore
  watcher.Dispose()
  source.Cancel()

// Generate
#if HELP
copyFiles()
buildDocumentation()
#endif
#if REFERENCE
copyFiles()
buildReference()
#endif
#if WATCH
watch()
#endif
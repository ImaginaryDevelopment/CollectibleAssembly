// https://www.strathweb.com/2019/01/collectible-assemblies-in-net-core-3-0/#:~:text=Collectible%20assemblies%20are%20implemented%20in%20.NET%20Core%203.0
// https://jeremybytes.blogspot.com/2020/01/dynamically-loading-types-in-net-core.html#:~:text=Create%20a%20custom%20assembly%20load%20context.%20Switch%20from%20using
// didn't load: https://jonathancrozier.com/blog/how-to-dynamically-load-different-versions-of-an-assembly-in-the-same-dot-net-application#:~:text=We%20can%20use%20the%20LoadFromAssemblyPath%20method%20on%20the
// https://siderite.dev/blog/dynamically-loading-types-from-assembly.html/#:~:text=.SelectMany(assembly%20=%3E%20assembly.GetTypes().Where(t%20=%3E

module Option =
    let ofEmptyList =
        function
        | [] -> None
        | v -> Some v

let mutable myCollectible = None
// let mutable myImpl = None

let getNext () =
    printfn ""
    printf "Cmd?"
    let txt = System.Console.ReadLine()
    Some(txt, Some txt)

type ImplementationType =
    | CImpl
    | FImpl

let findDll start (name: string) =
    System.IO.Directory.GetFiles(start, "*.dll", System.IO.SearchOption.AllDirectories)
    |> Seq.tryFind (fun fn -> fn.Contains name)
// |> fun v -> name, v

let loadAndRunAssembly it =
    match it, myCollectible with
    // don't reload the same context
    | CImpl, Some(CImpl, context, _) -> None
    | FImpl, Some(FImpl, context, _) -> None
    | CImpl, _ -> findDll @"..\..\src\CImpl\bin\" "CImpl.dll"
    | FImpl, _ -> findDll @"..\..\src\FImpl\bin\" "FImpl.dll"
    |> fun path ->
        match path with
        | None -> failwith "Unable to find path"
        | Some path ->
            let oldContext = myCollectible
            let fullPath = System.IO.Path.GetFullPath(path)
            printfn "Loading: '%s'" fullPath

            use ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath))
            let context = new System.Runtime.Loader.AssemblyLoadContext(string it, true)
            let asm = context.LoadFromStream(ms)
            myCollectible <- Some(it, context, asm)
            context.Assemblies |> Seq.length |> printfn "Loaded %i asm(s)"

            let asm =
                context.Assemblies
                |> Seq.choose (fun asm ->
                    printfn "Asm location is '%s'" asm.Location

                    [

                      asm.DefinedTypes
                      |> Seq.choose (fun et ->
                          printfn "Searching type: %s" et.FullName
                          et.GetInterface(nameof Schema.IAmImplementation, true) |> Option.ofObj)
                      |> List.ofSeq
                      |> List.tryHead

                      asm.ExportedTypes
                      |> Seq.choose (fun et ->
                          printfn "Searching type: %s" et.FullName
                          et.GetInterface(nameof Schema.IAmImplementation, true) |> Option.ofObj)
                      |> List.ofSeq
                      |> List.tryHead ]
                    |> List.choose id
                    |> List.tryHead)
                |> List.ofSeq
                |> List.tryHead

            match asm with
            | None -> eprintfn "No impl found in asm: '%s'" path
            | Some asm -> printfn "Found impl!"

            oldContext |> Option.iter (fun (_, c, _) -> c.Unload())



None
|> Seq.unfold (function
    | Some "" -> None
    | Some "C" ->
        loadAndRunAssembly CImpl
        getNext ()
    | Some "F" ->
        loadAndRunAssembly FImpl
        getNext ()
    | None -> getNext ()

    | Some _ -> getNext ())
|> Seq.takeWhile (function
    | "" -> false
    | _ -> true)
|> List.ofSeq
|> ignore

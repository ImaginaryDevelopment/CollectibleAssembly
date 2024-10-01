// https://www.strathweb.com/2019/01/collectible-assemblies-in-net-core-3-0/#:~:text=Collectible%20assemblies%20are%20implemented%20in%20.NET%20Core%203.0
// https://jeremybytes.blogspot.com/2020/01/dynamically-loading-types-in-net-core.html#:~:text=Create%20a%20custom%20assembly%20load%20context.%20Switch%20from%20using


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

let loadAndRunAssembly =
    function
    | CImpl -> "CImpl", findDll @"..\..\src\CImpl\bin\" "CImpl.dll"
    | FImpl -> "FImpl", findDll @"..\..\src\FImpl\bin\" "FImpl.dll"
    >> fun (name, path) ->
        match path with
        | None -> failwith "Unable to find path"
        | Some path ->
            let oldContext = myCollectible
            let fullPath = System.IO.Path.GetFullPath(path)
            printfn "Loading: '%s'" fullPath

            use ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(fullPath))
            let context = new System.Runtime.Loader.AssemblyLoadContext(name, true)
            myCollectible <- Some context
            let asm = context.Assemblies

            oldContext |> Option.iter (fun c -> c.Unload())



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

namespace FImpl

type MyFImpl() =
    interface Schema.IAmImplementation with
        member x.GetHello() : string = "Hello FImpl"

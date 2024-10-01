// See https://aka.ms/new-console-template for more information
using System.Linq;

var path = @"C:\dev\CollectibleAssembly\src\CImpl\bin\Debug\net8.0\CImpl.dll";

using var ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(path));
var context = new System.Runtime.Loader.AssemblyLoadContext("CImpl", collectible: true);
var asm = context.LoadFromStream(ms);

var t = asm.ExportedTypes.First(et => et.GetInterface(nameof(Schema.IAmImplementation)) != null);

Schema.IAmImplementation value = Activator.CreateInstance(t) as Schema.IAmImplementation;

Console.WriteLine(value.GetHello());

// won't actually do anything if references survive
context.Unload();

value.GetHello();

Console.WriteLine("Hello post unload?");

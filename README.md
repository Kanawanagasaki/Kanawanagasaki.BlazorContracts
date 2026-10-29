# Kanawanagasaki.BlazorContracts

## What?

A source-generator library where you define a contract and a handler, and the glue for Blazor WASM, Blazor Server, and SSR is generated automatically.

## Why?

Blazor WASM runs in the browser, while Blazor Server and SSR are server-rendered. This creates a complication when you want to execute the same method across different render modes: WASM must send an HTTP request, while Blazor Server and SSR retrieve data in-process. Blazor Contracts solves this problem by generating a service that you can call from WASM and the server side, routing all calls through the same handler.

## Structure

There are two NuGet packages:

[![NuGet package](https://img.shields.io/nuget/v/Kanawanagasaki.BlazorContracts.svg) Kanawanagasaki.BlazorContracts](https://www.nuget.org/packages/Kanawanagasaki.BlazorContracts)

[![NuGet package](https://img.shields.io/nuget/v/Kanawanagasaki.BlazorContracts.SourceGenerator.svg) Kanawanagasaki.BlazorContracts.SourceGenerator](https://www.nuget.org/packages/Kanawanagasaki.BlazorContracts.SourceGenerator)

The `Kanawanagasaki.BlazorContracts` package provides the common logic and attributes required for the source generator to discover contracts.

The `Kanawanagasaki.BlazorContracts.SourceGenerator` package provides four source generators that solve different parts of the equation:

| Type | What gets generated | Where to enable it |
|---|---|---|
| `Interface` | `IContractsService` - an interface that you inject into a page as a service, with overloads for all discovered contracts. | Shared project |
| `ClientService` | `ClientContractsService` - a client-side implementation of the `IContractsService` interface that generates HTTP requests for all discovered contracts. | Blazor WASM |
| `ServerService` | `ContractsService` - a server-side implementation of the `IContractsService` interface that calls the handlers directly for all discovered contracts. | Server |
| `Endpoints` | Generates minimal API endpoints that route requests to the handlers and takes care of all HTTP stuff. | Server |

## Setup

* Add the `Kanawanagasaki.BlazorContracts` package to a project that is accessible from both ASP.NET Core and Blazor projects.

```
dotnet package add Kanawanagasaki.BlazorContracts
```

* Add the `Kanawanagasaki.BlazorContracts.SourceGenerator` package to every project where you want the source generator to generate code.

```
dotnet package add Kanawanagasaki.BlazorContracts.SourceGenerator
```

**IMPORTANT!**

The source generator will not generate anything until you opt in by setting the `BlazorContracts` property in a property group and making it visible to the compiler with `CompilerVisibleProperty` in an item group.

```xml
<PropertyGroup>
  <BlazorContracts>...</BlazorContracts>
</PropertyGroup>
<ItemGroup>
  <CompilerVisibleProperty Include="BlazorContracts" />
</ItemGroup>
```

The value of `BlazorContracts` determines which source generators are enabled. Options are:

* `Interface` - generates `IContractsService`. Enable this option in a project accessible to both ASP.NET Core and Blazor WASM. Your contract classes must be visible to this project.
* `ClientService` - generates `ClientContractsService`. Enable this option in a Blazor WASM project. Your contract classes must be visible to this project.
* `ServerService` - generates `ContractsService`. Enable this option in an ASP.NET Core project. Both your contract classes and contract handlers must be visible to this project.
* `Endpoints` - generates ASP.NET Core minimal API endpoints. Enable this option in an ASP.NET Core project. Both your contract classes and contract handlers must be visible to this project.

You can enable multiple options in a project by separating them with commas. For example, an ASP.NET Core project will usually use both `ServerService` and `Endpoints`:

```
<PropertyGroup>
  <BlazorContracts>ServerService, Endpoints</BlazorContracts>
</PropertyGroup>
<ItemGroup>
  <CompilerVisibleProperty Include="BlazorContracts" />
</ItemGroup>
```

## First Contract

A contract is a class annotated with the `[Contract]` attribute and implements the `IContract` or `IContract<TResponse>` interface. The `[Contract]` attribute accepts two arguments: `string endpoint`, the minimal API endpoint, and `EVerbs verb`, which can be `Get`, `Post`, `Put`, or `Delete`. Use the non-generic `IContract` interface if you do not want the contract to carry any data. Use `IContract<TResponse>` if you want to specify a response type; `TResponse` is the contract's return type.

The simplest contract is a GET contract with the non-generic interface:

```csharp
[Contract("/my-contract", EVerbs.Get)]
public class MyContract : IContract { }
```

Create this class in a location where it is visible to both the ASP.NET Core project and Blazor WASM.

On the server, write one handler for each contract. Implement `IContractHandler<TContract>` when the call does not return a response, or `IContractHandler<TContract, TResult>` when it does.

```csharp
public class MyHandler : IContractHandler<MyContract>
{
    public Task<ContractResult> HandleAsync(MyContract contract, CancellationToken ct = default)
    {
        return Task.FromResult(new ContractResult(StatusCodes.Status204NoContent));
    }
}
```

Next, register the services on the server side:

```csharp
builder.Services.AddContracts();
```

Then map the endpoints:

```csharp
app.MapContracts();
```

Register the services on the client side:

```csharp
builder.Services.AddContracts();
```

Inject `IContractsService` into the page:

```razor
@inject IContractsService Contracts
```

or

```csharp
[Inject]
public required IContractsService Contracts { get; init; }
```

Finally, invoke the contract:

```csharp
var result = await Contracts.ProcessAsync(new MyContract());
```

## Handler Dependency Injection

Constructor parameters and public properties are resolved from DI by the generated code:

```csharp
public class MyHandler : IContractHandler<MyContract>
{
    public required MyService Service { get; init; }

    private readonly MyAnotherService _myAnotherService;

    public MyHandler(MyService myAnotherService)
    {
        _myAnotherService = myAnotherService;
    }
}
```

## Overriding the Defaults

The client-side `AddContracts()` also has an overload for a custom subclass:

```csharp
builder.Services.AddContracts<MyContractsService>();   // where T : ClientContractsService
```

Use this overload to override the default behavior or add hooks before and after key methods.

## The Result Class

Every call returns a `ContractResult` or `ContractResult<TData>` - an envelope that carries the outcome of the invocation:

```csharp
public class ContractResult<TData>
{
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public TData? Data { get; set; }

    public bool IsSuccess => StatusCode / 100 == 2;
    public bool IsSuccessWithData => IsSuccess && Data is not null;
}
```

## Route Parameters

Any `{xxx}` token in the endpoint template is bound from the property of the same name:

```csharp
[Contract("/my-contract/{Id}", EVerbs.Get)]
public class MyContract : IContract
{
    public int Id { get; init; }
}
```

The generated endpoint reconstructs the contract using `Id` from the route.

## Query Parameters

For `GET` and `DELETE` requests, every property not used by a route token becomes a query-string parameter:

```csharp
[Contract("/my-contract", EVerbs.Get)]
public class MyContract : IContract
{
    public int Page { get; set; }
    public int? Limit { get; set; }
    public string? Filter { get; set; }
}
```

During contract invocation, a contract like this:

```csharp
new MyContract
{
    Page = 2,
    Limit = 24,
    Filter = "Foo"
};
```

becomes the URL `/my-contract?Page=2&Limit=24&Filter=Foo`.

## Binary Uploads

To avoid the 33% increase in size caused by Base64 encoding binary properties in JSON, all `byte[]` properties are sent as binary multipart data alongside the JSON-encoded contract:

```csharp
[Contract("/my-contract", EVerbs.Post)]
public class MyContract : IContract
{
    public required byte[] Content { get; init; }
    public required string SomeOtherProperty { get; init; }
}
```

The `Content` property is excluded from JSON serialization. The generated endpoint reads the binary multipart data and assigns it to `Content`.

## ContractFile

Use `ContractFile` to upload a file from a component:

```csharp
[Contract("/my-contract", EVerbs.Post)]
public class MyContract : IContract
{
    public required ContractFile MyFile { get; init; }
}
```

```csharp
async Task UploadAsync(InputFileChangeEventArgs e)
{
    try
    {
        var file = e.File;
        if (file is null)
            return;

        await using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
        var contract = new MyContract
        {
            MyFile = new ContractFile(stream, file.Name, file.ContentType, file.Size)
        };
        var result = await Contracts.ProcessAsync(contract);
        if (result.IsSuccessWithData)
        {
            Console.WriteLine("Successful upload");
        }
        else
        {
            Console.WriteLine("Upload failed " + result.ErrorMessage);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Upload failed " + ex.Message);
    }
}
```

The file is streamed on both the client and server sides. Travels as binary multipart and excluded during  serialization.

---

## Binary Downloads

### Byte array

```csharp
[Contract("/my-contract", EVerbs.Get)] 
public class MyContract  : IContract<byte[]> { }
```

On success, byte-array contract responses are transferred as `application/octet-stream` and reassembled into contracts on the client side.

### Stream

```csharp
[Contract("/my-contract", EVerbs.Get)]
public class MyContract : IContract<Stream> { }
```

Like byte arrays, streams are transferred as `application/octet-stream` on success. Unlike byte arrays, however, streams deliver their bytes incrementally.

As you know streams must be dispossed of, but for handlers there is one rule: when returning a contract result, they transfer ownership of the stream to the generated endpoint and must **NOT** dispose of the stream themselves! Don't worry tho, the generated endpoint will dispose of the stream as soon as it finishes writing the response.

On the client side, the generated interface and `ClientContractsService` return a `DisposableContractResult<Stream>`, which you should dispose of as soon as you are done:

```csharp
await using var result = await Contracts.ProcessAsync(new MyContract());
if (result.IsSuccessWithData)
    await result.Data.CopyToAsync(someOutputStream);
```

### ContractFile

Returning `ContractFile` gives the response file-download semantics:

```csharp
[Contract("/my-contract", EVerbs.Get)]
public class MyContract : IContract<ContractFile> { }
```

When the request succeeds, the generated endpoint sets:

- `Content-Type` header using `ContractFile.MediaType`
- `Content-Disposition` header with filename from `ContractFile.FileName`
- `Content-Length` header using `ContractFile.Length`

You can use such contracts to deliver dynamically generated files.

As with a returned `Stream`, the handler should not dispose of the file's stream; the generated endpoint will do so. The client, however, should dispose of it.

## MessagePack

Annotating the contract with `[MessagePackObject]` switches the entire exchange to MessagePack format, when client and server are both C#, why use JSON, right?

```csharp
[Contract("/my-contract", EVerbs.Post)]
[MessagePackObject]
public class MyContract : IContract<MyResponse>
{
    [Key(0)]
    public int SomeNumber { get; init; }
    [Key(1)]
    public string? SomeString { get; init; }
    [Key(2)]
    public byte[] Binary { get; init; }
}
```

When a contract is annotated with `[MessagePackObject]`, both the contract and its response are encoded with MessagePack, so you better add `[MessagePackObject]` and `[Key(...)]` attributes to response class as well ;)

## Server-Sent Events (SSE)

A contract whose response is `IAsyncEnumerable<SseItem<T>>` becomes an SSE endpoint:

```csharp
[Contract("/my-contract", EVerbs.Get)]
public class MyContract : IContract<IAsyncEnumerable<SseItem<MyEvent>>> { }
```

On the server, return a `ContractResult` containing the `IAsyncEnumerable<SseItem<MyEvent>>`:

```csharp
public class MyHandler : IContractHandler<MyContract, IAsyncEnumerable<SseItem<MyEvent>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<MyEvent>>>> HandleAsync(MyContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<MyEvent>>>(EnumerateAsync(ct)));

    private async IAsyncEnumerable<SseItem<MyEvent>> EnumerateAsync([EnumeratorCancellation] CancellationToken ct)
    {
        for (int i = 0; i < 100; i++)
        {
            var myEvent = new MyEvent(i);
            yield return new SseItem<MyEvent>(myEvent, "my-event");

            await Task.Delay(1000, ct);
        }
    }
}
```

On the client side, consume it like any other asynchronous enumerable:

```csharp
var result = await Contracts.ProcessAsync(new MyContract(), ct);
if (result.IsSuccessWithData)
{
    await foreach (var item in result.Data.WithCancellation(ct))
    {
        if (item.EventType == "my-event")
            Console.WriteLine(item.Data.Id);
    }
}
```

## LZ4 Compression

For funzies there is the `[ContractCompression(LZ4Level.…)]` attribute, if you'll add it to a contract then during the wire transfer contract will be compressed using LZ4.

```csharp
[Contract("my-contract", EVerbs.Get)]
[ContractCompression(LZ4Level.L03_HC)]
public class MyContract : IContract { }
```

This works with JSON, MessagePack, binary return types and SSE. Be mindful that `ContractFile` responses are also compressed with LZ4, which browsers have no idea how to decompress automatically.

## Authentication & Authorization

Decorate handlers with standard ASP.NET Core attributes. The generator detects these attributes and enforces them:

```csharp
[Authorize(Roles = "Admin")]
public class MyAdminHandler : IContractHandler<MyAdminContract> { ... }

[Authorize(Policy = "MySuperPolicy")]
public class MyAuthPolicyHandler : IContractHandler<MyAuthPolicyContract> { ... }

[Authorize(AuthenticationSchemes = "...")]
public class MyAuthSchemeHandler : IContractHandler<MyAuthSchemeContract> { ... }
```

**The in-process service supports this too!** The generated `ProcessAsync` method performs the equivalent checks before invoking the handler.

## Endpoint Attributes

Attributes on a handler class, other than `[Authorize]`, are applied to the generated minimal API endpoint:

```csharp
[ExcludeFromDescription]
[OutputCache(NoStore = true)]
[IgnoreAntiforgeryToken]
public class MyHandler : IContractHandler<MyContract> { ... }
```

### License

[MIT](LICENSE.txt).

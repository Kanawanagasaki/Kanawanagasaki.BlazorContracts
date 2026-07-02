using Kanawanagasaki.BlazorContracts.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddContracts();

await builder.Build().RunAsync();

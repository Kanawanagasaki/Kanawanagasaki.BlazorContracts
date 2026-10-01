namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class CustomDisposableResource : IDisposable
{
    public byte[] Content { get; set; } = [];

    public void Dispose() { }
}

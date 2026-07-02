namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/todos/{Id}", EVerbs.Delete)]
public class TodoDeleteContract : IContract
{
    public int Id { get; set; }
}

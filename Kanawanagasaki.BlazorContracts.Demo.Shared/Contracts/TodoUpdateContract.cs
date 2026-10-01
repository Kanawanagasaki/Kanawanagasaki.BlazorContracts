namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/todos/{Id}", EVerbs.Put)]
public class TodoUpdateContract : IContract<TodoItem>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsDone { get; init; }
}

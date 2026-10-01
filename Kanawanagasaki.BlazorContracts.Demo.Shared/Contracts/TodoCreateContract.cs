namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/todos", EVerbs.Post)]
public class TodoCreateContract : IContract<TodoItem>
{
    public string Title { get; init; } = string.Empty;
    public bool IsDone { get; init; }
}

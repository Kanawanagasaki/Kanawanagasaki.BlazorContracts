namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/todos", EVerbs.Get)]
public class TodoGetAllContract : IContract<TodoItem[]>
{
    public bool? FilterIsDone { get; set; }
}

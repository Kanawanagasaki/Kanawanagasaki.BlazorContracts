namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/todos/{Id}", EVerbs.Get)]
public class TodoGetByIdContract : IContract<TodoItem>
{
    public int Id { get; set; }
}

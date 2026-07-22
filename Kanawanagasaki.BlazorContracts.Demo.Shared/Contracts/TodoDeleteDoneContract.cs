namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/todos/done", EVerbs.Delete)]
public class TodoDeleteDoneContract : IContract<TodoItem[]>
{
    public int? OlderThanDays { get; set; }
    public bool DryRun { get; set; }
}

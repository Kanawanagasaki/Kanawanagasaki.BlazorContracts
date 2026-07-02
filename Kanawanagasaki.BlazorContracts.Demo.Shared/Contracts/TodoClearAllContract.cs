namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/todos/clear", EVerbs.Post)]
public class TodoClearAllContract : IContract { }

namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Kanawanagasaki.BlazorContracts.Demo.Stores;
using Kanawanagasaki.BlazorContracts;

public class TodoDeleteDoneHandler(AppStore store) : BaseLoggerHandler, IContractHandler<TodoDeleteDoneContract, TodoItem[]>
{
    private readonly AppStore _store = store;

    public Task<ContractResult<TodoItem[]>> HandleAsync(TodoDeleteDoneContract contract, CancellationToken ct = default)
    {
        var deleted = _store.DeleteDoneTodos(contract.OlderThanDays, contract.DryRun);
        return Task.FromResult(new ContractResult<TodoItem[]>(deleted));
    }
}

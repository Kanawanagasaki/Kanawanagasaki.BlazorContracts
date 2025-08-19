namespace Kanawanagasaki.BlazorContracts;

public interface IContractHandler<TContract> where TContract : IContract
{
    public Task<ContractResult> HandleAsync(TContract contract, CancellationToken ct = default);
}

public interface IContractHandler<TContract, TResult> where TContract : IContract<TResult>
{
    public Task<ContractResult<TResult>> HandleAsync(TContract contract, CancellationToken ct = default);
}

namespace Kanawanagasaki.BlazorContracts;

public interface IContractsService
{
    Task<ContractResult> ProcessAsync(IContract req);
    Task<ContractResult<TResponse>> ProcessAsync<TResponse>(IContract<TResponse> req) where TResponse : class;
}

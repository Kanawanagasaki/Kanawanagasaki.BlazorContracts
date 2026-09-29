namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

/// <summary>
/// Demonstrates the MessagePack contract + JSON response mixed mode. The handler itself
/// doesn't care about wire format — it just returns the result, and the generated endpoint
/// picks the serializer based on which side carries [MessagePackObject].
/// </summary>
public class MessagePackContractJsonResponseHandler : IContractHandler<MessagePackContractJsonResponseContract, SimpleTestResponse>
{
    public Task<ContractResult<SimpleTestResponse>> HandleAsync(MessagePackContractJsonResponseContract contract, CancellationToken ct = default)
    {
        var response = new SimpleTestResponse
        {
            Message = $"Echo({contract.Input}) #{contract.Echo}"
        };
        return Task.FromResult(new ContractResult<SimpleTestResponse>(response));
    }
}

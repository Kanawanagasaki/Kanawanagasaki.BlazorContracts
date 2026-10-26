namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Text;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class TestGetMsgpackCompressedComplexHandler : IContractHandler<TestGetMsgpackCompressedComplexContract, ComplexResponse>
{
    public Task<ContractResult<ComplexResponse>> HandleAsync(TestGetMsgpackCompressedComplexContract contract, CancellationToken ct = default)
    {
        var seed = contract.Id * 31
            + Fnv1aUtf8(contract.Tag)
            + (contract.Limit ?? 0)
            + contract.Offset
            + (int)contract.Scope;

        var response = ComplexResponse.Create(seed);
        response.Name = $"{response.Name}|{contract.Id}|{contract.Tag}|{contract.Limit}|{contract.Offset}|{contract.Scope}";

        return Task.FromResult(new ContractResult<ComplexResponse>(response));
    }

    private static int Fnv1aUtf8(string? value)
    {
        if (value is null)
            return 0;

        uint hash = 0x811c9dc5;
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            hash ^= b;
            hash = unchecked(hash * 16777619);
        }
        return unchecked((int)hash);
    }
}

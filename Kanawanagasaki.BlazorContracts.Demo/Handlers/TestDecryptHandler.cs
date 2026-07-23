namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using System.Security.Cryptography;

public class TestDecryptHandler : IContractHandler<TestDecryptContract, byte[]>
{
    readonly static byte[] Key =
    [
        0xA8, 0x8E, 0x54, 0xCA, 0x35, 0x48, 0x27, 0x52,
        0x59, 0x63, 0xCB, 0x4C, 0x80, 0x1C, 0x61, 0x12,
        0x4B, 0x23, 0x90, 0x6A, 0x61, 0x82, 0x71, 0x33
    ];

    public Task<ContractResult<byte[]>> HandleAsync(TestDecryptContract contract, CancellationToken ct = default)
    {
        if (contract.Ciphertext is null || contract.Ciphertext.Length == 0)
            return Task.FromResult(new ContractResult<byte[]>(StatusCodes.Status400BadRequest, "Ciphertext is required."));

        if (contract.Nonce is null || contract.Nonce.Length == 0)
            return Task.FromResult(new ContractResult<byte[]>(StatusCodes.Status400BadRequest, "Nonce/IV is required."));

        if (contract.Tag is null || contract.Tag.Length == 0)
            return Task.FromResult(new ContractResult<byte[]>(StatusCodes.Status400BadRequest, "Tag is required."));

        byte[] plaintext = new byte[contract.Ciphertext.Length];

        using var aesGcm = new AesGcm(Key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Decrypt(contract.Nonce, contract.Ciphertext, contract.Tag, plaintext);

        return Task.FromResult(new ContractResult<byte[]>(plaintext));
    }
}

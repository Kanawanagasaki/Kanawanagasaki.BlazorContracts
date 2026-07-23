namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using System.Security.Cryptography;

public class TestEncryptHandler : IContractHandler<TestEncryptContract, CryptoResult>
{
    readonly static byte[] Key =
    [
        0xA8, 0x8E, 0x54, 0xCA, 0x35, 0x48, 0x27, 0x52,
        0x59, 0x63, 0xCB, 0x4C, 0x80, 0x1C, 0x61, 0x12,
        0x4B, 0x23, 0x90, 0x6A, 0x61, 0x82, 0x71, 0x33
    ];

    public Task<ContractResult<CryptoResult>> HandleAsync(TestEncryptContract contract, CancellationToken ct = default)
    {
        if (contract.Plaintext is null || contract.Plaintext.Length == 0)
            return Task.FromResult(new ContractResult<CryptoResult>(StatusCodes.Status400BadRequest, "Plaintext is required."));

        byte[] ciphertext = new byte[contract.Plaintext.Length];
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aesGcm = new AesGcm(Key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, contract.Plaintext, ciphertext, tag);

        var result = new CryptoResult
        {
            Ciphertext = ciphertext,
            Nonce = nonce,
            Tag = tag
        };

        return Task.FromResult(new ContractResult<CryptoResult>(result));
    }
}

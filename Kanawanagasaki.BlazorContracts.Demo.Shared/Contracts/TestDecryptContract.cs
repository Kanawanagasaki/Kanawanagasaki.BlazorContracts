namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/crypto/decrypt", EVerbs.Post)]
public class TestDecryptContract : IContract<byte[]>
{
    public byte[] Ciphertext { get; init; } = [];
    public byte[] Nonce { get; init; } = [];
    public byte[] Tag { get; init; } = [];
}

namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/test/crypto/encrypt", EVerbs.Post)]
public class TestEncryptContract : IContract<CryptoResult>
{
    public byte[] Plaintext { get; set; } = [];
}

namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class CryptoResult
{
    public byte[] Ciphertext { get; set; } = [];
    public byte[] Nonce { get; set; } = [];
    public byte[] Tag { get; set; } = [];
}

namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

public static class Constants
{
    public const string IContractFullName = "Kanawanagasaki.BlazorContracts.IContract";
    public const string IContractHandlerFullName = "Kanawanagasaki.BlazorContracts.IContractHandler";
    public const string ContractAttributeFullName = "Kanawanagasaki.BlazorContracts.ContractAttribute";
    public const string ContractResultFullName = "Kanawanagasaki.BlazorContracts.ContractResult";
    public const string DisposableContractResultFullName = "Kanawanagasaki.BlazorContracts.DisposableContractResult";

    public const string IAsyncDisposableFullName = "System.IAsyncDisposable";

    public const string JsonPropertyNameAttributeFullName = "System.Text.Json.Serialization.JsonPropertyNameAttribute";

    public const string MessagePackObjectAttributeFullName = "MessagePack.MessagePackObjectAttribute";

    public const string ContractCompressionAttributeFullName = "Kanawanagasaki.BlazorContracts.ContractCompressionAttribute";
    public const string LZ4CompressionLevel = "K4os.Compression.LZ4.LZ4Level";
    public const string LZ4CompressionLevelDefault = LZ4CompressionLevel + ".L00_FAST";
}

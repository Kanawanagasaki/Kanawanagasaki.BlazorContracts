namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public sealed class ContractCompressionAttribute : Attribute
{
    public LZ4Level Level { get; }

    public ContractCompressionAttribute(LZ4Level level = LZ4Level.L00_FAST)
    {
        Level = level;
    }
}

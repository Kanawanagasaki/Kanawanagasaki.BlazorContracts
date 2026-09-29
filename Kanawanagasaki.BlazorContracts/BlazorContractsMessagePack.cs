namespace Kanawanagasaki.BlazorContracts;

using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

public static class BlazorContractsMessagePack
{
    public static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create([new ContractFileFormatter()], [StandardResolver.Instance, ContractlessStandardResolver.Instance]));

    public static byte[] Serialize<T>(T value)
        => MessagePackSerializer.Serialize(value, Options);

    public static Task SerializeAsync<T>(Stream destination, T value, CancellationToken ct = default)
        => MessagePackSerializer.SerializeAsync(destination, value, Options, ct);

    public static T? Deserialize<T>(byte[] source)
    {
        if (source is null || source.Length == 0)
            return default;
        return MessagePackSerializer.Deserialize<T>(source, Options);
    }

    public static T? Deserialize<T>(ReadOnlyMemory<byte> source)
        => MessagePackSerializer.Deserialize<T>(source, Options);

    public static async ValueTask<T?> DeserializeAsync<T>(Stream source, CancellationToken ct = default)
        => await MessagePackSerializer.DeserializeAsync<T>(source, Options, ct);

    public sealed class ContractFileFormatter : IMessagePackFormatter<ContractFile?>
    {
        public void Serialize(ref MessagePackWriter writer, ContractFile? value, MessagePackSerializerOptions options)
        {
            writer.WriteNil();
        }

        public ContractFile? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            reader.Skip();
            return null;
        }
    }
}

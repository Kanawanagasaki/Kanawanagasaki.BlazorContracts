namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Kanawanagasaki.BlazorContracts.Demo.Services;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

internal static class ChatSseStream
{
    internal static async IAsyncEnumerable<SseItem<ChatMessage>> EnumerateAsync(ChatBroadcaster chat, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var (id, reader) = chat.Subscribe();
        try
        {
            yield return new SseItem<ChatMessage>(null, "ping");

            while (await reader.WaitToReadAsync(ct))
                while (reader.TryRead(out var message))
                    yield return new SseItem<ChatMessage>(message, "msg");
        }
        finally
        {
            chat.Unsubscribe(id);
        }
    }
}

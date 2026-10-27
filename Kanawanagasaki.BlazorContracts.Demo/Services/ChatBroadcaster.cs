namespace Kanawanagasaki.BlazorContracts.Demo.Services;

using System.Collections.Concurrent;
using System.Threading.Channels;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public sealed class ChatBroadcaster
{
    private readonly ConcurrentDictionary<Guid, Channel<ChatMessage>> _subscribers = new();

    public (Guid Id, ChannelReader<ChatMessage> Reader) Subscribe()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<ChatMessage>(new UnboundedChannelOptions { SingleReader = true });
        _subscribers[id] = channel;
        return (id, channel.Reader);
    }

    public void Unsubscribe(Guid id)
        => _subscribers.TryRemove(id, out _);

    public void Broadcast(ChatMessage message)
    {
        foreach (var subscriber in _subscribers)
            subscriber.Value.Writer.TryWrite(message);
    }
}

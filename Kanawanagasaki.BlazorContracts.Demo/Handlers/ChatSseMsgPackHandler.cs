namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Services;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class ChatSseMsgPackHandler(ChatBroadcaster chat) : IContractHandler<ChatSseMsgPackContract, IAsyncEnumerable<SseItem<ChatMessage>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<ChatMessage>>>> HandleAsync(ChatSseMsgPackContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<ChatMessage>>>(ChatSseStream.EnumerateAsync(chat, ct)));
}

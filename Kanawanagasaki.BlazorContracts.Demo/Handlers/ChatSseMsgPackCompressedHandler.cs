namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Services;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class ChatSseMsgPackCompressedHandler(ChatBroadcaster chat) : IContractHandler<ChatSseMsgPackCompressedContract, IAsyncEnumerable<SseItem<ChatMessage>>>
{
    public Task<ContractResult<IAsyncEnumerable<SseItem<ChatMessage>>>> HandleAsync(ChatSseMsgPackCompressedContract contract, CancellationToken ct = default)
        => Task.FromResult(new ContractResult<IAsyncEnumerable<SseItem<ChatMessage>>>(ChatSseStream.EnumerateAsync(chat, ct)));
}

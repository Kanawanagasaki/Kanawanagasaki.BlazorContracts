namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Services;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class ChatSendHandler(ChatBroadcaster chat) : IContractHandler<ChatSendContract>
{
    public Task<ContractResult> HandleAsync(ChatSendContract contract, CancellationToken ct = default)
    {
        var nickname = contract.Nickname?.Trim() ?? string.Empty;
        var text = contract.Text?.Trim() ?? string.Empty;

        if (nickname.Length == 0 || 64 < nickname.Length)
            return Task.FromResult(new ContractResult(400, "Nickname must be 1-64 characters."));

        if (text.Length == 0 || 500 < text.Length)
            return Task.FromResult(new ContractResult(400, "Message must be 1-500 characters."));

        chat.Broadcast(new ChatMessage { Nickname = nickname, Text = text, SentAt = DateTime.UtcNow });
        return Task.FromResult(new ContractResult(200));
    }
}

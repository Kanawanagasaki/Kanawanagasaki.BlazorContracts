namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/chat/send", EVerbs.Post)]
public class ChatSendContract : IContract
{
    public string Nickname { get; init; } = string.Empty;

    public string Text { get; init; } = string.Empty;
}

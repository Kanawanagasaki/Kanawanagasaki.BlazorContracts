namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

using MessagePack;

[MessagePackObject]
public class ChatMessage
{
    [Key(0)]
    public string Nickname { get; set; } = string.Empty;

    [Key(1)]
    public string Text { get; set; } = string.Empty;

    [Key(2)]
    public DateTime SentAt { get; set; }
}

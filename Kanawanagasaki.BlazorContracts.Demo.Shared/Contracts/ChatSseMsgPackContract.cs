namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/chat/sse/msgpack", EVerbs.Get)]
[MessagePackObject]
public class ChatSseMsgPackContract : IContract<IAsyncEnumerable<SseItem<ChatMessage>>> { }

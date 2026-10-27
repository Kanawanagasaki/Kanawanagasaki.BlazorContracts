namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using MessagePack;

[Contract("/api/chat/sse/msgpack-compressed", EVerbs.Get)]
[MessagePackObject]
[ContractCompression]
public class ChatSseMsgPackCompressedContract : IContract<IAsyncEnumerable<SseItem<ChatMessage>>> { }

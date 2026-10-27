namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/chat/sse/compressed-json", EVerbs.Get)]
[ContractCompression]
public class ChatSseCompressedJsonContract : IContract<IAsyncEnumerable<SseItem<ChatMessage>>> { }

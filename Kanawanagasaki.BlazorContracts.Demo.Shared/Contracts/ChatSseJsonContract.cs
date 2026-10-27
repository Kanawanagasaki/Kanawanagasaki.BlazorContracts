namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/chat/sse/json", EVerbs.Get)]
public class ChatSseJsonContract : IContract<IAsyncEnumerable<SseItem<ChatMessage>>> { }

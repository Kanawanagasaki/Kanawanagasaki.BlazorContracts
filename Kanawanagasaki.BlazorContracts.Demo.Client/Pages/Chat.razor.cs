namespace Kanawanagasaki.BlazorContracts.Demo.Client.Pages;

using System.Net.ServerSentEvents;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Microsoft.AspNetCore.Components;

public partial class Chat : ComponentBase, IDisposable
{
    private enum EFormat { Json, MessagePack }
    private enum ECompression { None, Lz4 }

    [Inject]
    public required IContractsService Contracts { get; init; }

    private sealed record Entry(ChatMessage? Message, string? Status);

    private readonly List<Entry> _entries = new();
    private string _nickname = "";
    private string _draft = "";
    private EFormat _format = EFormat.Json;
    private ECompression _compression = ECompression.None;
    private bool _connecting;
    private bool _connected;
    private string? _error;
    private CancellationTokenSource? _cts;

    private string TransportLabel
        => $"{(_format == EFormat.MessagePack ? "MessagePack" : "JSON")}{(_compression == ECompression.Lz4 ? " + LZ4 compression" : "")}";

    private void ConnectAsync()
    {
        _error = null;
        var nickname = _nickname.Trim();
        if (nickname.Length == 0)
        {
            _error = "Nickname is required.";
            return;
        }
        if (64 < nickname.Length)
        {
            _error = "Nickname must be at most 64 characters.";
            return;
        }

        _nickname = nickname;
        _entries.Clear();
        _connecting = true;
        StateHasChanged();

        _cts = new CancellationTokenSource();
        _ = ReceiveLoopAsync(_cts.Token);
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            var result = (_format, _compression) switch
            {
                (EFormat.Json, ECompression.None) => await Contracts.ProcessAsync(new ChatSseJsonContract(), ct),
                (EFormat.Json, ECompression.Lz4) => await Contracts.ProcessAsync(new ChatSseCompressedJsonContract(), ct),
                (EFormat.MessagePack, ECompression.None) => await Contracts.ProcessAsync(new ChatSseMsgPackContract(), ct),
                _ => await Contracts.ProcessAsync(new ChatSseMsgPackCompressedContract(), ct),
            };

            if (!result.IsSuccess || result.Data is null)
            {
                await LogAsync($"Connection failed: {result.StatusCode} {result.ErrorMessage}");
                return;
            }

            _connected = true;
            await LogAsync($"Connected as {_nickname} via {TransportLabel}.");

            await foreach (var item in result.Data.WithCancellation(ct))
            {
                if (item.EventType == "msg")
                    await LogAsync(null, item.Data);
            }

            await LogAsync("Disconnected.");
        }
        catch (OperationCanceledException)
        {
            await LogAsync("Disconnected.");
        }
        catch (Exception e)
        {
            await LogAsync($"Connection problem: {e.Message}");
        }
        finally
        {
            _connected = false;
            _connecting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SendAsync()
    {
        var text = _draft.Trim();
        if (text.Length == 0)
            return;

        var result = await Contracts.ProcessAsync(new ChatSendContract { Nickname = _nickname, Text = text });
        if (!result.IsSuccess)
            await LogAsync($"Send failed: {result.StatusCode} {result.ErrorMessage}");
        else
        {
            _draft = "";
            await InvokeAsync(StateHasChanged);
        }
    }

    private Task LogAsync(string? status, ChatMessage? message = null)
        => InvokeAsync(() =>
        {
            _entries.Add(new Entry(message, status));
            StateHasChanged();
        });

    public void Dispose()
        => _cts?.Cancel();
}

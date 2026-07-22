namespace Kanawanagasaki.BlazorContracts.Demo.Stores;

using System.Collections.Concurrent;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

public class AppStore
{
    private readonly ConcurrentDictionary<int, TodoItem> _todos = new();
    public IReadOnlyDictionary<int, TodoItem> Todos
        => _todos;
    private int _nextTodoId = 0;

    private readonly ConcurrentDictionary<int, StoredFile> _files = new();
    public IReadOnlyDictionary<int, StoredFile> Files
        => _files;
    private int _nextFileId = 0;

    public TodoItem[] GetAllTodos()
        => _todos.Values.OrderBy(t => t.Id).ToArray();

    public TodoItem[] GetAllTodos(bool isDone)
        => _todos.Values.Where(t => t.IsDone == isDone).OrderBy(t => t.Id).ToArray();

    public TodoItem? GetTodoById(int id)
        => _todos.GetValueOrDefault(id);

    public TodoItem AddTodo(string title, bool isDone)
    {
        var id = Interlocked.Increment(ref _nextTodoId);
        var todo = new TodoItem
        {
            Id = id,
            Title = title,
            IsDone = isDone,
            CreatedAt = DateTime.UtcNow
        };
        _todos[id] = todo;
        return todo;
    }

    public TodoItem? UpdateTodo(int id, string title, bool isDone)
    {
        if (!_todos.TryGetValue(id, out var existing))
            return null;

        existing.Title = title;
        existing.IsDone = isDone;
        _todos[id] = existing;
        return existing;
    }

    public bool DeleteTodo(int id)
        => _todos.TryRemove(id, out _);

    public TodoItem[] DeleteDoneTodos(int? olderThanDays = null, bool dryRun = false)
    {
        var now = DateTime.UtcNow;
        var matching = _todos.Values
            .Where(t => t.IsDone)
            .Where(t => !olderThanDays.HasValue || (now - t.CreatedAt).TotalDays >= olderThanDays.Value)
            .OrderBy(t => t.Id)
            .ToArray();

        if (!dryRun)
        {
            foreach (var t in matching)
                _todos.TryRemove(t.Id, out _);
        }

        return matching;
    }

    public void ClearTodos()
        => _todos.Clear();

    public (int id, StoredFile file) StoreFile(string fileName, string mediaType, byte[] content)
    {
        var id = Interlocked.Increment(ref _nextFileId);
        var file = new StoredFile
        {
            Id = id,
            FileName = fileName,
            MediaType = mediaType,
            Content = content,
            Length = content.Length,
            UploadedAt = DateTime.UtcNow,
            Sha256 = ComputeSha256(content)
        };
        _files[id] = file;
        return (id, file);
    }

    public StoredFile? GetFileById(int id)
        => _files.TryGetValue(id, out var f) ? f : null;

    private static string ComputeSha256(byte[] data)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(data);
        return Convert.ToHexString(hash);
    }

    public void Seed()
    {
        if (_todos.IsEmpty)
        {
            AddTodo("Learn Blazor Contracts", false);
            AddTodo("Try prerendering + SignalR + WASM", false);
            AddTodo("Build a CRUD page", true);
            AddTodo("Read the query parameter docs", true);
            AddTodo("Star the repo on GitHub", true);
        }
    }
}

public class StoredFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public long Length { get; set; }
    public DateTime UploadedAt { get; set; }
    public string Sha256 { get; set; } = string.Empty;
}

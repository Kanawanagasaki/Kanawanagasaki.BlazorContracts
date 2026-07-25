namespace Kanawanagasaki.BlazorContracts;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public class ContractResult
{
    [JsonIgnore]
    public bool IsSuccess => StatusCode / 100 == 2;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    public ContractResult() { }

    public ContractResult(int statusCode)
    {
        StatusCode = statusCode;
    }

    public ContractResult(int statusCode, string? errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }
}

public class ContractResult<TData> : ContractResult
{
    [JsonPropertyName("data")]
    public TData? Data { get; set; }

    [JsonIgnore, MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccessWithData => IsSuccess && Data is not null;

    public ContractResult() : base() { }

    public ContractResult(int statusCode) : base(statusCode) { }

    public ContractResult(int statusCode, string? errorMessage) : base(statusCode, errorMessage) { }

    public ContractResult(TData data) : base(200)
    {
        Data = data;
    }

    public DisposableContractResult<TData> AsDisposable()
        => new DisposableContractResult<TData>(StatusCode, ErrorMessage, Data);

    public static implicit operator ContractResult<TData>(TData data)
        => new ContractResult<TData>(data);
}

public class DisposableContractResult<TData> : ContractResult<TData>, IDisposable, IAsyncDisposable
{
    [JsonIgnore]
    public HttpResponseMessage? HttpResponse { get; set; }

    public DisposableContractResult() : base() { }

    public DisposableContractResult(int statusCode, string? errorMessage) : base(statusCode, errorMessage) { }

    public DisposableContractResult(TData data) : base(data) { }

    public DisposableContractResult(int statusCode, string? errorMessage, TData? data = default) : base(200, errorMessage)
    {
        Data = data;
    }

    public virtual void Dispose()
    {
        if (Data is IDisposable disposableData)
            disposableData.Dispose();

        HttpResponse?.Dispose();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (Data is IAsyncDisposable asyncDisposableData)
            await asyncDisposableData.DisposeAsync();
        else if (Data is IDisposable disposableData)
            disposableData.Dispose();

        HttpResponse?.Dispose();
    }
}

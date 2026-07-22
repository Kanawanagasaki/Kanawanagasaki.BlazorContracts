namespace Kanawanagasaki.BlazorContracts;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public class ContractResult : IDisposable
{
    [JsonIgnore]
    public bool IsSuccess => StatusCode / 100 == 2;

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonIgnore]
    public HttpResponseMessage? HttpResponse { get; set; }

    public ContractResult()
    {
        StatusCode = 200;
    }

    public ContractResult(int statusCode)
    {
        StatusCode = statusCode;
    }

    public ContractResult(int statusCode, string? errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public virtual void Dispose()
    {
        HttpResponse?.Dispose();
    }
}

public class ContractResult<TData> : ContractResult, IAsyncDisposable
{
    [JsonPropertyName("data")]
    public TData? Data { get; set; }

    [JsonIgnore, MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccessWithData => IsSuccess && Data is not null;

    public ContractResult() : base(200) { }

    public ContractResult(int statusCode) : base(statusCode) { }

    public ContractResult(int statusCode, string? errorMessage) : base(statusCode, errorMessage) { }

    public ContractResult(TData data) : base(200)
    {
        Data = data;
    }

    public static implicit operator ContractResult<TData>(TData data)
        => new ContractResult<TData>(data);

    public override void Dispose()
    {
        if (Data is IDisposable disposableData)
            disposableData.Dispose();

        base.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (Data is IAsyncDisposable asyncDisposableData)
            await asyncDisposableData.DisposeAsync();
        else if (Data is IDisposable disposableData)
            disposableData.Dispose();

        base.Dispose();
    }
}

//public class DisposableContractResult<TData> : ContractResult<TData>, IDisposable, IAsyncDisposable
//{
//    [JsonIgnore]
//    public HttpResponseMessage? HttpResponse { get; set; }

//    public virtual void Dispose()
//    {
//        if (Data is IDisposable disposableData)
//            disposableData.Dispose();

//        HttpResponse?.Dispose();
//    }

//    public virtual async ValueTask DisposeAsync()
//    {
//        if (Data is IAsyncDisposable asyncDisposableData)
//            await asyncDisposableData.DisposeAsync();
//        else if (Data is IDisposable disposableData)
//            disposableData.Dispose();

//        HttpResponse?.Dispose();
//    }
//}

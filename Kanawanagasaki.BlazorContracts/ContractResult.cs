namespace Kanawanagasaki.BlazorContracts;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public class ContractResult
{
    [JsonIgnore]
    public bool IsSuccess => StatusCode / 100 == 2;

    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }

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
}

public class ContractResult<TData> : ContractResult
{
    public TData? Data { get; set; }

    [MemberNotNullWhen(true, nameof(Data))]
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
}

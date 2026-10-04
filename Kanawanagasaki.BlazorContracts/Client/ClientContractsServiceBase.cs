namespace Kanawanagasaki.BlazorContracts.Client;

using System.Net;
using System.Text.Json;

public abstract class ClientContractsServiceBase
{
    private readonly HttpClient _http;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected ClientContractsServiceBase(HttpClient http, JsonSerializerOptions jsonOptions)
    {
        _http = http;
        _jsonOptions = jsonOptions;
    }

    protected async Task<ContractResult<T>> ProcessCoreAsync<T>(HttpRequestMessage request, CancellationToken ct = default)
    {
        static ContractResult<T> factory(int s, string? e) => new(s, e);

        try
        {
            using var response = await SendAsync(request, ct);
            return await ReadEnvelopeAsync(response, factory, ct);
        }
        catch (OperationCanceledException)
        {
            return factory(499, "Client Closed Request");
        }
        catch (Exception)
        {
            return factory(0, "Network Error");
        }
    }

    protected async Task<ContractResult> ProcessCoreAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        static ContractResult factory(int s, string? e) => new(s, e);

        try
        {
            using var response = await SendAsync(request, ct);
            return await ReadEnvelopeAsync(response, factory, ct);
        }
        catch (OperationCanceledException)
        {
            return factory(499, "Client Closed Request");
        }
        catch (Exception)
        {
            return factory(0, "Network Error");
        }
    }

    protected async Task<DisposableContractResult<T>> ProcessDisposableCoreAsync<T>(HttpRequestMessage request, CancellationToken ct = default)
    {
        static DisposableContractResult<T> factory(int s, string? e) => new(s, e);

        HttpResponseMessage? response = null;
        try
        {
            response = await SendAsync(request, ct);
            var result = await ReadEnvelopeAsync(response, factory, ct);
            result.HttpResponse = response;
            response = null;
            return result;
        }
        catch (OperationCanceledException)
        {
            return factory(499, "Client Closed Request");
        }
        catch (Exception)
        {
            return factory(0, "Network Error");
        }
        finally
        {
            response?.Dispose();
        }
    }

    protected async Task<ContractResult<byte[]>> ProcessBytesCoreAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        try
        {
            using var response = await SendAsync(request, ct);
            return await ReadBytesAsync(response, ct);
        }
        catch (OperationCanceledException)
        {
            return new ContractResult<byte[]>(499, "Client Closed Request");
        }
        catch (Exception)
        {
            return new ContractResult<byte[]>(0, "Network Error");
        }
    }

    protected async Task<DisposableContractResult<Stream>> ProcessStreamCoreAsync(HttpRequestMessage request, CancellationToken ct = default)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await SendAsync(request, ct);
            var result = await ReadStreamAsync(response, ct);
            result.HttpResponse = response;
            response = null;
            return result;
        }
        catch (OperationCanceledException)
        {
            return new DisposableContractResult<Stream>(499, "Client Closed Request");
        }
        catch (Exception)
        {
            return new DisposableContractResult<Stream>(0, "Network Error");
        }
        finally
        {
            response?.Dispose();
        }
    }

    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

    private async Task<TResult> ReadEnvelopeAsync<TResult>(HttpResponseMessage response, Func<int, string?, TResult> resultFactory, CancellationToken ct)
        where TResult : ContractResult
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return resultFactory((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is not null && !ContractMediaTypes.IsJson(mediaType) && !ContractMediaTypes.IsMessagePack(mediaType))
            return resultFactory((int)response.StatusCode, "Unexpected content type " + mediaType);

        var contentStream = await response.Content.ReadAsStreamAsync(ct);
        var result = await ContractBody.ParseAsync<TResult>(contentStream, mediaType, _jsonOptions, compressed: ContractLz4.IsLz4Response(response), ct: ct);
        return result ?? resultFactory((int)response.StatusCode, "Failed to deserialize the response");
    }

    private async Task<ContractResult<byte[]>> ReadBytesAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new ContractResult<byte[]>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        var compressed = ContractLz4.IsLz4Response(response);

        if (ContractMediaTypes.IsJson(mediaType) || ContractMediaTypes.IsMessagePack(mediaType))
        {
            var contentStream = await response.Content.ReadAsStreamAsync(ct);
            var result = await ContractBody.ParseAsync<ContractResult<byte[]>>(contentStream, mediaType, _jsonOptions, compressed: compressed, ct: ct);
            return result ?? new ContractResult<byte[]>((int)response.StatusCode, "Failed to deserialize the response");
        }

        if (ContractMediaTypes.IsMultipart(mediaType))
            return await ReadMultipartBytesAsync(response, ct);

        var stream = await response.Content.ReadAsStreamAsync(ct);
        using var payload = new MemoryStream();
        if (compressed)
        {
            await using var decoder = ContractLz4.CreateDecoderStream(stream, leaveOpen: true);
            await decoder.CopyToAsync(payload, ct);
        }
        else
        {
            await stream.CopyToAsync(payload, ct);
        }
        return new((int)response.StatusCode) { Data = payload.ToArray() };
    }

    private async Task<DisposableContractResult<Stream>> ReadStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new DisposableContractResult<Stream>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (ContractMediaTypes.IsJson(mediaType) || ContractMediaTypes.IsMessagePack(mediaType))
        {
            var contentStream = await response.Content.ReadAsStreamAsync(ct);
            var result = await ContractBody.ParseAsync<DisposableContractResult<Stream>>(contentStream, mediaType, _jsonOptions, compressed: ContractLz4.IsLz4Response(response), ct: ct);
            return result ?? new DisposableContractResult<Stream>((int)response.StatusCode, "Failed to deserialize the response");
        }

        var stream = await response.Content.ReadAsStreamAsync(ct);
        if (ContractLz4.IsLz4Response(response))
            stream = ContractLz4.CreateDecoderStream(stream);
        return new DisposableContractResult<Stream>(stream);
    }

    private async Task<ContractResult<byte[]>> ReadMultipartBytesAsync(HttpResponseMessage response, CancellationToken ct)
    {
        ContractResult<byte[]>? contractResult = null;
        byte[]? byteArray = null;

        var body = await response.Content.ReadAsStreamAsync(ct);
        var parts = await ContractMultipart.ParseAsync(body, response.Content.Headers.ContentType?.ToString(), ct);
        if (parts is not null)
        {
            foreach (var part in parts.Values)
            {
                try
                {
                    if (ContractMediaTypes.IsJson(part.ContentType) || ContractMediaTypes.IsMessagePack(part.ContentType))
                        contractResult ??= await ContractBody.ParseAsync<ContractResult<byte[]>>(part.Stream, part.ContentType, _jsonOptions, ct: ct);
                    else
                    {
                        using var payload = new MemoryStream();
                        await part.Stream.CopyToAsync(payload, ct);
                        byteArray ??= payload.ToArray();
                    }
                }
                finally
                {
                    await part.DisposeAsync();
                }

                if (contractResult is not null && byteArray is not null)
                    break;
            }
        }

        if (contractResult is null && byteArray is null)
            return new ContractResult<byte[]>((int)response.StatusCode, "Failed to parse the multipart response");
        if (byteArray is null)
            return contractResult!;
        if (contractResult is null)
            return new((int)response.StatusCode) { Data = byteArray };
        contractResult.Data = byteArray;
        return contractResult;
    }
}

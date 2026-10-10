namespace Kanawanagasaki.BlazorContracts;

using System.Net;
using System.Text.Json;

public abstract class ClientContractsServiceBase
{
    protected abstract HttpClient Http { get; }
    protected abstract JsonSerializerOptions JsonOptions { get; }

    private Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

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

    private async Task<TResult> ReadEnvelopeAsync<TResult>(HttpResponseMessage response, Func<int, string?, TResult> resultFactory, CancellationToken ct) where TResult : ContractResult
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return resultFactory((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        var isMessagePack = mediaType is not null && ContractMediaTypes.IsMessagePack(mediaType);
        if (!isMessagePack && !ContractMediaTypes.IsJson(mediaType))
            return resultFactory((int)response.StatusCode, "Unexpected content type " + mediaType);

        var contentStream = await response.Content.ReadAsStreamAsync(ct);
        TResult? result;
        if (ContractLz4.IsLz4Response(response))
        {
            await using var decompressor = ContractLz4.CreateDecoderStream(contentStream, true);
            if (isMessagePack)
                result = await BlazorContractsMessagePack.DeserializeAsync<TResult>(decompressor, ct);
            else
                result = await JsonSerializer.DeserializeAsync<TResult>(decompressor, JsonOptions, ct);
        }
        else
        {
            if (isMessagePack)
                result = await BlazorContractsMessagePack.DeserializeAsync<TResult>(contentStream, ct);
            else
                result = await JsonSerializer.DeserializeAsync<TResult>(contentStream, JsonOptions, ct);
        }

        return result ?? resultFactory((int)response.StatusCode, "Failed to deserialize the response");
    }

    private async Task<ContractResult<byte[]>> ReadBytesAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new ContractResult<byte[]>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (ContractMediaTypes.IsMultipart(mediaType))
            return await ReadMultipartBytesAsync(response, ct);

        var stream = await response.Content.ReadAsStreamAsync(ct);
        Stream? decompressor = null;
        if (ContractLz4.IsLz4Response(response))
            decompressor = stream = ContractLz4.CreateDecoderStream(stream, true);

        try
        {
            if (ContractMediaTypes.IsBinary(mediaType))
            {
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                return new((int)response.StatusCode) { Data = ms.ToArray() };
            }

            if (ContractMediaTypes.IsJson(mediaType))
            {
                var result = await JsonSerializer.DeserializeAsync<ContractResult<byte[]>>(stream, JsonOptions, ct);
                return result ?? new ContractResult<byte[]>((int)response.StatusCode, "Failed to deserialize the response");
            }

            if (ContractMediaTypes.IsMessagePack(mediaType))
            {
                var result = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<byte[]>>(stream, ct);
                return result ?? new ContractResult<byte[]>((int)response.StatusCode, "Failed to deserialize the response");
            }

            return new ContractResult<byte[]>((int)response.StatusCode, "Unexpected content type " + mediaType);
        }
        finally
        {
            if (decompressor is not null)
                await decompressor.DisposeAsync();
        }
    }

    private async Task<ContractResult<byte[]>> ReadMultipartBytesAsync(HttpResponseMessage response, CancellationToken ct)
    {
        ContractResult<byte[]>? contractResult = null;
        byte[]? byteArray = null;

        await using var body = await response.Content.ReadAsStreamAsync(ct);

        await foreach (var part in ContractMultipart.ParseAsync(body, response.Content.Headers.ContentType?.ToString(), ct))
        {
            try
            {
                if (ContractMediaTypes.IsBinary(part.ContentType))
                {
                    using var ms = new MemoryStream();
                    await part.Stream.CopyToAsync(ms, ct);
                    byteArray ??= ms.ToArray();
                }
                else if (ContractMediaTypes.IsJson(part.ContentType))
                    contractResult ??= await JsonSerializer.DeserializeAsync<ContractResult<byte[]>>(part.Stream, JsonOptions, ct);
                else if (ContractMediaTypes.IsMessagePack(part.ContentType))
                    contractResult ??= await BlazorContractsMessagePack.DeserializeAsync<ContractResult<byte[]>>(part.Stream, ct);
            }
            finally
            {
                await part.DisposeAsync();
            }

            if (contractResult is not null && byteArray is not null)
                break;
        }

        if (contractResult is null && byteArray is null)
            return new ContractResult<byte[]>((int)response.StatusCode, "Failed to parse the response");
        if (byteArray is null)
            return contractResult!;
        if (contractResult is null)
            return new((int)response.StatusCode) { Data = byteArray };
        contractResult.Data = byteArray;
        return contractResult;
    }

    private async Task<DisposableContractResult<Stream>> ReadStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new DisposableContractResult<Stream>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (ContractMediaTypes.IsMultipart(mediaType))
            return await ReadMultipartStreamAsync(response, ct);

        var contentStream = await response.Content.ReadAsStreamAsync(ct);
        Stream? decompressor = null;
        if (ContractLz4.IsLz4Response(response))
            decompressor = contentStream = ContractLz4.CreateDecoderStream(contentStream, true);

        try
        {
            if (ContractMediaTypes.IsBinary(mediaType))
            {
                decompressor = null;
                return new((int)response.StatusCode)
                {
                    Data = contentStream,
                    HttpResponse = response
                };
            }

            if (ContractMediaTypes.IsJson(mediaType))
            {
                var result = await JsonSerializer.DeserializeAsync<DisposableContractResult<Stream>>(contentStream, JsonOptions, ct);
                result ??= new DisposableContractResult<Stream>((int)response.StatusCode, "Failed to deserialize the response");
                result.HttpResponse = response;
                return result;
            }

            if (ContractMediaTypes.IsMessagePack(mediaType))
            {
                var result = await BlazorContractsMessagePack.DeserializeAsync<DisposableContractResult<Stream>>(contentStream, ct);
                result ??= new DisposableContractResult<Stream>((int)response.StatusCode, "Failed to deserialize the response");
                result.HttpResponse = response;
                return result;
            }

            return new DisposableContractResult<Stream>((int)response.StatusCode, "Unexpected content type " + mediaType)
            {
                HttpResponse = response
            };
        }
        finally
        {
            if (decompressor is not null)
                await decompressor.DisposeAsync();
        }
    }

    private async Task<DisposableContractResult<Stream>> ReadMultipartStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        DisposableContractResult<Stream>? contractResult = null;
        Stream? streamData = null;

        await using var body = await response.Content.ReadAsStreamAsync(ct);

        await foreach (var part in ContractMultipart.ParseAsync(body, response.Content.Headers.ContentType?.ToString(), ct))
        {
            if (ContractMediaTypes.IsBinary(part.ContentType))
                streamData ??= part.Stream;
            else
            {
                try
                {
                    if (ContractMediaTypes.IsJson(part.ContentType))
                        contractResult ??= await JsonSerializer.DeserializeAsync<DisposableContractResult<Stream>>(part.Stream, JsonOptions, ct);
                    else if (ContractMediaTypes.IsMessagePack(part.ContentType))
                        contractResult ??= await BlazorContractsMessagePack.DeserializeAsync<DisposableContractResult<Stream>>(part.Stream, ct);
                }
                finally
                {
                    await part.DisposeAsync();
                }
            }

            if (contractResult is not null && streamData is not null)
                break;
        }

        if (contractResult is null && streamData is null)
        {
            return new DisposableContractResult<Stream>((int)response.StatusCode, "Failed to parse the response")
            {
                HttpResponse = response
            };
        }

        if (streamData is null)
        {
            contractResult!.HttpResponse = response;
            return contractResult!;
        }

        if (contractResult is null)
        {
            return new((int)response.StatusCode)
            {
                Data = streamData,
                HttpResponse = response
            };
        }

        contractResult.Data = streamData;
        contractResult.HttpResponse = response;

        return contractResult;
    }
}

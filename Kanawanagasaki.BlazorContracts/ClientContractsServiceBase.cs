namespace Kanawanagasaki.BlazorContracts;

using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
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

    private async Task<TResult> ReadEnvelopeAsync<TResult>(HttpResponseMessage response, Func<int, string?, TResult> resultFactory, CancellationToken ct) where TResult : ContractResult
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return resultFactory((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        var isMessagePack = mediaType is not null && ContractHeaders.IsMessagePack(mediaType);
        if (!isMessagePack && !ContractHeaders.IsJson(mediaType))
            return resultFactory((int)response.StatusCode, "Unexpected content type " + mediaType);

        var contentStream = await response.Content.ReadAsStreamAsync(ct);
        TResult? result;
        if (ContractHeaders.IsLz4Response(response))
        {
            await using var decompressor = LZ4Stream.Decode(contentStream, leaveOpen: true);
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

    private async Task<ContractResult<byte[]>> ReadBytesAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new ContractResult<byte[]>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (ContractHeaders.IsJson(mediaType) || ContractHeaders.IsMessagePack(mediaType))
            return await ReadEnvelopeAsync(response, (statusCode, errorMessage) => new ContractResult<byte[]>(statusCode, errorMessage), ct);
        if (ContractHeaders.IsMultipart(mediaType))
            return await ReadMultipartBytesAsync(response, ct);
        if (!ContractHeaders.IsBinary(mediaType))
            return new((int)response.StatusCode, "Unexpected content type " + mediaType);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var ms = new MemoryStream();
        if (ContractHeaders.IsLz4Response(response))
        {
            await using var decompressor = LZ4Stream.Decode(stream);
            await decompressor.CopyToAsync(ms, ct);
        }
        else
        {
            await stream.CopyToAsync(ms, ct);
        }
        return new((int)response.StatusCode) { Data = ms.ToArray() };
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
                if (ContractHeaders.IsBinary(part.ContentType))
                {
                    using var ms = new MemoryStream();
                    await part.Stream.CopyToAsync(ms, ct);
                    byteArray ??= ms.ToArray();
                }
                else if (ContractHeaders.IsJson(part.ContentType))
                    contractResult ??= await JsonSerializer.DeserializeAsync<ContractResult<byte[]>>(part.Stream, JsonOptions, ct);
                else if (ContractHeaders.IsMessagePack(part.ContentType))
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

    private async Task<DisposableContractResult<Stream>> ReadStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new DisposableContractResult<Stream>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (ContractHeaders.IsJson(mediaType) || ContractHeaders.IsMessagePack(mediaType))
            return await ReadEnvelopeAsync(response, (statusCode, errorMessage) => new DisposableContractResult<Stream>(statusCode, errorMessage), ct);
        if (ContractHeaders.IsMultipart(mediaType))
            return await ReadMultipartStreamAsync(response, ct);
        if (!ContractHeaders.IsBinary(mediaType))
        {
            return new((int)response.StatusCode, "Unexpected content type " + mediaType)
            {
                HttpResponse = response
            };
        }

        var contentStream = await response.Content.ReadAsStreamAsync(ct);
        if (ContractHeaders.IsLz4Response(response))
            contentStream = LZ4Stream.Decode(contentStream);

        return new((int)response.StatusCode)
        {
            Data = contentStream,
            HttpResponse = response
        };
    }

    private async Task<DisposableContractResult<Stream>> ReadMultipartStreamAsync(HttpResponseMessage response, CancellationToken ct)
    {
        DisposableContractResult<Stream>? contractResult = null;
        Stream? streamData = null;

        await using var body = await response.Content.ReadAsStreamAsync(ct);

        await foreach (var part in ContractMultipart.ParseAsync(body, response.Content.Headers.ContentType?.ToString(), ct))
        {
            if (ContractHeaders.IsBinary(part.ContentType))
                streamData ??= part.Stream;
            else
            {
                try
                {
                    if (ContractHeaders.IsJson(part.ContentType))
                        contractResult ??= await JsonSerializer.DeserializeAsync<DisposableContractResult<Stream>>(part.Stream, JsonOptions, ct);
                    else if (ContractHeaders.IsMessagePack(part.ContentType))
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

    protected async Task<DisposableContractResult<ContractFile>> ProcessFileCoreAsync(HttpRequestMessage request, string filename, CancellationToken ct = default)
    {
        HttpResponseMessage? response = null;
        try
        {
            response = await SendAsync(request, ct);
            var result = await ReadFileAsync(response, filename, ct);
            result.HttpResponse = response;
            response = null;
            return result;
        }
        catch (OperationCanceledException)
        {
            return new DisposableContractResult<ContractFile>(499, "Client Closed Request");
        }
        catch (Exception)
        {
            return new DisposableContractResult<ContractFile>(0, "Network Error");
        }
        finally
        {
            response?.Dispose();
        }
    }

    private async Task<DisposableContractResult<ContractFile>> ReadFileAsync(HttpResponseMessage response, string filename, CancellationToken ct = default)
    {
        if (response.StatusCode is HttpStatusCode.NoContent)
            return new DisposableContractResult<ContractFile>((int)response.StatusCode, null);

        var mediaType = response.Content.Headers.ContentType?.MediaType;

        if (ContractHeaders.IsJson(mediaType) || ContractHeaders.IsMessagePack(mediaType))
            return await ReadEnvelopeAsync(response, (statusCode, errorMessage) => new DisposableContractResult<ContractFile>(statusCode, errorMessage), ct);
        if (ContractHeaders.IsMultipart(mediaType))
            return await ReadMultipartFileAsync(response, ct);

        var contentDisposition = ContractHeaders.GetContentDisposition(response);
        var contentFilename = ContractHeaders.GetFileNameFromDisposition(contentDisposition) ?? filename;

        var contentLength = -1L;
        var contentStream = await response.Content.ReadAsStreamAsync(ct);
        if (ContractHeaders.IsLz4Response(response))
        {
            contentStream = LZ4Stream.Decode(contentStream);
            contentLength = ContractHeaders.GetXOriginalSize(response) ?? -1;
        }
        else if (response.Content.Headers.ContentLength.HasValue)
            contentLength = response.Content.Headers.ContentLength.Value;

        return new DisposableContractResult<ContractFile>((int)response.StatusCode)
        {
            Data = new ContractFile(contentStream, contentFilename, mediaType ?? ContractHeaders.OctetStreamMediaType, contentLength),
            HttpResponse = response
        };
    }

    private async Task<DisposableContractResult<ContractFile>> ReadMultipartFileAsync(HttpResponseMessage response, CancellationToken ct)
    {
        DisposableContractResult<ContractFile>? contractResult = null;
        ContractFile? file = null;

        await using var body = await response.Content.ReadAsStreamAsync(ct);

        await foreach (var part in ContractMultipart.ParseAsync(body, response.Content.Headers.ContentType?.ToString(), ct))
        {
            if (ContractHeaders.IsJson(part.ContentType) || ContractHeaders.IsMessagePack(part.ContentType))
            {
                try
                {
                    if (ContractHeaders.IsJson(part.ContentType))
                        contractResult ??= await JsonSerializer.DeserializeAsync<DisposableContractResult<ContractFile>>(part.Stream, JsonOptions, ct);
                    else
                        contractResult ??= await BlazorContractsMessagePack.DeserializeAsync<DisposableContractResult<ContractFile>>(part.Stream, ct);
                }
                finally
                {
                    await part.DisposeAsync();
                }
            }
            else if (file is null)
            {
                file = new ContractFile(part.Stream, part.FileName ?? "file", part.ContentType ?? ContractHeaders.OctetStreamMediaType, part.Length, leaveOpen: false);
            }
            else
            {
                await part.DisposeAsync();
            }

            if (contractResult is not null && file is not null)
                break;
        }

        if (contractResult is null && file is null)
        {
            return new DisposableContractResult<ContractFile>((int)response.StatusCode, "Failed to parse the response")
            {
                HttpResponse = response
            };
        }

        if (file is null)
        {
            contractResult!.HttpResponse = response;
            return contractResult!;
        }

        if (contractResult is null)
        {
            return new((int)response.StatusCode)
            {
                Data = file,
                HttpResponse = response
            };
        }

        contractResult.Data = file;
        contractResult.HttpResponse = response;
        return contractResult;
    }
}

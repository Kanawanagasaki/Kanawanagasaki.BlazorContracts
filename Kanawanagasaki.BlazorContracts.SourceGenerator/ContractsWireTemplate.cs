namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

internal static class ContractsWireTemplate
{
    internal const string Source = """
        internal static class ContractsWire
        {
            internal static async System.Threading.Tasks.Task<T?> ParseBodyAsync<T>(
                Microsoft.AspNetCore.Http.HttpRequest request,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                bool preferMessagePack,
                System.Threading.CancellationToken ct = default) where T : class
            {
                var compressed = request.Headers.TryGetValue(Kanawanagasaki.BlazorContracts.ContractLz4.HeaderName, out var encodingValues)
                    && encodingValues.Any(Kanawanagasaki.BlazorContracts.ContractLz4.IsLz4);
                return await Kanawanagasaki.BlazorContracts.ContractBody.ParseAsync<T>(
                    request.Body,
                    request.ContentType,
                    jsonOptions,
                    preferMessagePack: preferMessagePack,
                    compressed: compressed,
                    ct: ct);
            }

            internal static async System.Threading.Tasks.Task WriteBadRequestAsync(
                Microsoft.AspNetCore.Http.HttpResponse response,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                bool messagePack,
                bool compressed,
                K4os.Compression.LZ4.LZ4Level compressionLevel,
                System.Threading.CancellationToken ct = default)
            {
                await WriteResultAsync(response, new Kanawanagasaki.BlazorContracts.ContractResult(400, "Bad Request"), jsonOptions, messagePack, compressed, compressionLevel, ct);
            }

            internal static async System.Threading.Tasks.Task WriteErrorAsync(
                Microsoft.AspNetCore.Http.HttpResponse response,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                bool messagePack,
                bool compressed,
                K4os.Compression.LZ4.LZ4Level compressionLevel,
                System.Threading.CancellationToken ct = default)
            {
                await WriteResultAsync(response, new Kanawanagasaki.BlazorContracts.ContractResult(500, "Internal Server Error"), jsonOptions, messagePack, compressed, compressionLevel, ct);
            }

            internal static async System.Threading.Tasks.Task WriteResultAsync<T>(
                Microsoft.AspNetCore.Http.HttpResponse response,
                T result,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                bool messagePack,
                bool compressed,
                K4os.Compression.LZ4.LZ4Level compressionLevel,
                System.Threading.CancellationToken ct = default) where T : Kanawanagasaki.BlazorContracts.ContractResult
            {
                response.StatusCode = result.StatusCode;
                if (result.StatusCode == Microsoft.AspNetCore.Http.StatusCodes.Status204NoContent)
                    return;

                response.ContentType = messagePack
                    ? Kanawanagasaki.BlazorContracts.ContractMediaTypes.MessagePack
                    : Kanawanagasaki.BlazorContracts.ContractMediaTypes.JsonCharset;
                if (compressed)
                    response.Headers[Kanawanagasaki.BlazorContracts.ContractLz4.HeaderName] = Kanawanagasaki.BlazorContracts.ContractLz4.HeaderValue;

                await WritePayloadAsync(response.Body, result, messagePack, compressed, compressionLevel, jsonOptions, ct);
            }

            internal static async System.Threading.Tasks.Task WritePayloadAsync<T>(
                System.IO.Stream target,
                T payload,
                bool messagePack,
                bool compressed,
                K4os.Compression.LZ4.LZ4Level compressionLevel,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                System.Threading.CancellationToken ct = default)
            {
                if (compressed)
                {
                    await using var encoder = Kanawanagasaki.BlazorContracts.ContractLz4.CreateEncoderStream(target, compressionLevel, leaveOpen: true);
                    await WritePayloadCoreAsync(encoder, payload, messagePack, jsonOptions, ct);
                    await encoder.FlushAsync(ct);
                    return;
                }

                await WritePayloadCoreAsync(target, payload, messagePack, jsonOptions, ct);
            }

            private static async System.Threading.Tasks.Task WritePayloadCoreAsync<T>(
                System.IO.Stream target,
                T payload,
                bool messagePack,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                System.Threading.CancellationToken ct = default)
            {
                if (messagePack)
                    await Kanawanagasaki.BlazorContracts.BlazorContractsMessagePack.SerializeAsync(target, payload, ct);
                else
                    await System.Text.Json.JsonSerializer.SerializeAsync(target, payload, jsonOptions, ct);
            }

            private static byte[] Utf8(string value)
                => System.Text.Encoding.UTF8.GetBytes(value);

            private static string Lz4HeaderLine(bool compressed)
                => compressed ? $"{Kanawanagasaki.BlazorContracts.ContractLz4.HeaderName}: {Kanawanagasaki.BlazorContracts.ContractLz4.HeaderValue}\r\n" : string.Empty;

            internal static async System.Threading.Tasks.Task WriteBytesResultAsync(
                Microsoft.AspNetCore.Http.HttpResponse response,
                Kanawanagasaki.BlazorContracts.ContractResult<byte[]> result,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                bool messagePack,
                bool compressed,
                K4os.Compression.LZ4.LZ4Level compressionLevel,
                string contractName,
                System.Threading.CancellationToken ct = default)
            {
                if (messagePack || result.Data is null || result.ErrorMessage is not null)
                {
                    await WriteResultAsync(response, result, jsonOptions, messagePack, compressed, compressionLevel, ct);
                    return;
                }

                var data = result.Data;
                var boundary = "boundary" + System.Guid.NewGuid().ToString("N");
                response.StatusCode = result.StatusCode;
                response.ContentType = $"multipart/related; boundary=\"{boundary}\"";

                await response.Body.WriteAsync(Utf8(
                    $"--{boundary}\r\n" +
                    $"Content-Type: application/json; charset=utf-8\r\n" +
                    $"Content-Disposition: inline; name=\"{contractName}\"\r\n" +
                    Lz4HeaderLine(compressed) +
                    $"\r\n"), ct);
                await WritePayloadAsync(response.Body, result, messagePack: false, compressed, compressionLevel, jsonOptions, ct);

                // Binary part.
                await response.Body.WriteAsync(Utf8(
                    $"\r\n--{boundary}\r\n" +
                    $"Content-Type: application/octet-stream\r\n" +
                    $"Content-Disposition: attachment; filename=\"Data.bin\"\r\n" +
                    $"Content-Transfer-Encoding: binary\r\n" +
                    Lz4HeaderLine(compressed) +
                    $"\r\n"), ct);
                if (compressed)
                {
                    await using var encoder = Kanawanagasaki.BlazorContracts.ContractLz4.CreateEncoderStream(response.Body, compressionLevel, leaveOpen: true);
                    await encoder.WriteAsync(data, ct);
                    await encoder.FlushAsync(ct);
                }
                else
                {
                    await response.Body.WriteAsync(data, ct);
                }

                await response.Body.WriteAsync(Utf8($"\r\n--{boundary}--\r\n"), ct);
            }

            internal static async System.Threading.Tasks.Task WriteStreamResultAsync(
                Microsoft.AspNetCore.Http.HttpResponse response,
                Kanawanagasaki.BlazorContracts.ContractResult<System.IO.Stream> result,
                System.Text.Json.JsonSerializerOptions jsonOptions,
                bool messagePack,
                bool compressed,
                K4os.Compression.LZ4.LZ4Level compressionLevel,
                System.Threading.CancellationToken ct = default)
            {
                if (result.Data is not null && result.ErrorMessage is null)
                {
                    response.StatusCode = result.StatusCode;
                    response.ContentType = Kanawanagasaki.BlazorContracts.ContractMediaTypes.Binary;

                    if (result.Data.CanSeek)
                        result.Data.Position = 0;

                    if (compressed)
                    {
                        response.Headers[Kanawanagasaki.BlazorContracts.ContractLz4.HeaderName] = Kanawanagasaki.BlazorContracts.ContractLz4.HeaderValue;
                        await using var encoder = Kanawanagasaki.BlazorContracts.ContractLz4.CreateEncoderStream(response.Body, compressionLevel, leaveOpen: true);
                        await result.Data.CopyToAsync(encoder, ct);
                        await encoder.FlushAsync(ct);
                        return;
                    }

                    if (result.Data.CanSeek)
                        response.ContentLength = result.Data.Length;

                    await result.Data.CopyToAsync(response.Body, ct);
                    return;
                }

                await WriteResultAsync(response, result, jsonOptions, messagePack, compressed, compressionLevel, ct);
            }
        }
        """;
}

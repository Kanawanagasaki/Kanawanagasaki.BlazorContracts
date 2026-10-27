namespace Kanawanagasaki.BlazorContracts.Demo.Client.Pages;

using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Streams;
using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.ServerSentEvents;
using System.Text;
using System.Text.Json;

public partial class Test : ComponentBase
{
    [Inject]
    public required NavigationManager NavMgr { get; init; }
    [Inject]
    public required IContractsService Contracts { get; init; }

    private readonly List<CategoryResult> _categories = new();
    private readonly List<LogEntry> _logs = new();
    private int _passedTestsCount;
    private bool _isRunning;

    protected override async Task OnInitializedAsync()
    {
        await RunTestsAsync();
    }

    private async Task RunTestsAsync()
    {
        _isRunning = true;
        StateHasChanged();

        try
        {
            _logs.Add(new LogEntry("Testing...", null));
            _passedTestsCount = 0;
            _categories.Clear();

            await RunBasicsAsync();
            await RunMessagePackAsync();
            await RunFilesAsync();
            await RunContractFileAsync();
            await RunContractFileDisposalAsync();
            await RunGeneratorEdgeCasesAsync();
            await RunQueryOptionalityAsync();
            await RunCompressedAsync();
            await RunTodoCrudAsync();
            await RunWeatherAsync();
            await RunSseAsync();

            if (RendererInfo.Name == "WebAssembly")
            {
                await RunWireFormatsAsync();
                await RunMisalignedServerAsync();
                await RunSseWireFormatsAsync();
            }

            _logs.Add(new LogEntry($"Done. {_passedTestsCount} tests passed.", true));
        }
        catch (Exception ex)
        {
            _logs.Add(new LogEntry(ex.ToString(), false));
        }

        _isRunning = false;
        StateHasChanged();
    }

    private void ClearLogs()
    {
        _logs.Clear();
        StateHasChanged();
    }

    private void Pass(CategoryResult cat, string label, bool pass, string detail)
    {
        cat.Items.Add(new CategoryItem(label, pass, detail));
        if (pass)
            _passedTestsCount++;
        else
            _logs.Add(new LogEntry($"{label} FAILED: {detail}", false));
    }

    private async Task RunCaseAsync(CategoryResult cat, string label, Func<Task> body)
    {
        try
        {
            await body();
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"{label} threw: {ex.Message}", false));
            Console.WriteLine(ex);
        }
        StateHasChanged();
    }

    // Wire-only: raw-HTTP endpoint tests. The endpoint-over-real-HTTP check belongs to the
    // WASM pass (ClientContractsService + raw HttpClient both go over the network). In SSR the
    // typed handler tests already run in-process and the endpoint is not what a Server pass is
    // supposed to verify — so these raw tests are intentionally skipped there.
    private async Task RunCaseWireAsync(CategoryResult cat, string label, Func<Task> body)
    {
        if (RendererInfo.Name != "WebAssembly")
            return;
        await RunCaseAsync(cat, label, body);
    }

    private void Recount()
        => _passedTestsCount = _categories.Sum(c => c.Passed);

    private void CountLast(CategoryResult cat)
    {
        if (0 < cat.Items.Count && cat.Items[^1].Pass)
            _passedTestsCount++;
    }

    private async Task RunBasicsAsync()
    {
        var cat1 = new CategoryResult("GET - No Body");
        await TestGetNoData(cat1, "GET no data (204)");
        CountLast(cat1);

        var cat2 = new CategoryResult("GET - Simple Response");
        await TestGetSimpleResponse(cat2, "GET simple response");
        CountLast(cat2);

        var cat3 = new CategoryResult("GET - Complex Response");
        await TestGetComplexResponse(cat3, "GET complex response");
        CountLast(cat3);

        var cat4 = new CategoryResult("GET - Stream Response");
        await TestGetStreamResponse(cat4, "GET stream response");
        CountLast(cat4);

        var cat5 = new CategoryResult("GET - Route Parameters");
        await TestGetRouteParamNoResponse(cat5, "GET route param no response");
        CountLast(cat5);
        await TestGetRouteQueryParam(cat5, "GET route + query params");
        CountLast(cat5);

        var cat6 = new CategoryResult("POST - Simple");
        await TestPostNoData(cat6, "POST no data");
        CountLast(cat6);
        await TestPostNoBodyNoResponse(cat6, "POST no body/no response");
        CountLast(cat6);

        var cat7 = new CategoryResult("POST - With Body Verification");
        await TestPostSimpleBody(cat7, "POST simple body");
        CountLast(cat7);
        await TestPostComplexBody(cat7, "POST complex body");
        CountLast(cat7);

        var cat8 = new CategoryResult("POST - AES Encryption");
        await TestEncryptDecryptRoundTrip(cat8);
        foreach (var item in cat8.Items)
            if (item.Pass) _passedTestsCount++;

        var cat9 = new CategoryResult("POST - File Upload");
        await TestUploadSingleFile(cat9, "upload single file");
        CountLast(cat9);
        await TestUploadMultipleFiles(cat9, "upload multiple files");
        CountLast(cat9);
        await TestUploadCombined(cat9, "upload byte array + files combined");
        CountLast(cat9);

        var catA = new CategoryResult("PUT - With Body");
        await TestPutSimpleBody(catA, "PUT simple body");
        CountLast(catA);
        await TestPutRouteParam(catA, "PUT route param + body");
        CountLast(catA);

        var catB = new CategoryResult("DELETE");
        await TestDeleteNoBodyNoResponse(catB, "DELETE no response");
        CountLast(catB);

        var catC = new CategoryResult("POST - LZ4 Compression");
        await TestCompressEcho(catC, "compressed json echo");
        CountLast(catC);
        await TestCompressBinary(catC, "compressed binary parts");
        CountLast(catC);
        await TestCompressMessagePack(catC, "compressed message pack");
        CountLast(catC);

        _categories.AddRange(new[] { cat1, cat2, cat3, cat4, cat5, cat6, cat7, cat8, cat9, catA, catB, catC });
        Recount();
        StateHasChanged();
    }

    private async Task RunMessagePackAsync()
    {
        var cat = new CategoryResult("MessagePack (typed)");

        await RunCaseAsync(cat, "msgpack weather round trip (inline bin payload)", async () =>
        {
            var payload = new byte[1024];
            Random.Shared.NextBytes(payload);

            var result = await Contracts.ProcessAsync(new MessagePackWeatherContract { Seed = 42, Days = 4, Payload = payload });

            var pass = result.IsSuccessWithData
                && result.Data.Payload.SequenceEqual(payload)
                && result.Data.Days is { Length: 4 }
                && !string.IsNullOrEmpty(result.Data.Summary);

            Pass(cat, "msgpack weather round trip (inline bin payload)", pass,
                $"payload={result.Data?.Payload.Length ?? 0}B days={result.Data?.Days?.Length ?? 0} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "json contract -> msgpack response (mixed)", async () =>
        {
            var payload = new byte[512];
            Random.Shared.NextBytes(payload);

            var result = await Contracts.ProcessAsync(new JsonContractMessagePackResponseContract { Seed = 7, Days = 3, Payload = payload });

            var pass = result.IsSuccessWithData
                && result.Data.Payload.SequenceEqual(payload)
                && result.Data.Days is { Length: 3 };

            Pass(cat, "json contract -> msgpack response (mixed)", pass,
                $"payload={result.Data?.Payload.Length ?? 0}B days={result.Data?.Days?.Length ?? 0} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "msgpack contract -> json response (mixed)", async () =>
        {
            var result = await Contracts.ProcessAsync(new MessagePackContractJsonResponseContract { Input = "wire", Echo = 99 });

            var pass = result.IsSuccessWithData && result.Data.Message == "Echo(wire) #99";

            Pass(cat, "msgpack contract -> json response (mixed)", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "msgpack bytes upload (single msgpack body)", async () =>
        {
            var payload = new byte[2048];
            Random.Shared.NextBytes(payload);
            var sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload));

            var result = await Contracts.ProcessAsync(new MessagePackBytesUploadContract
            {
                FileName = "typed-bytes.bin",
                MediaType = "application/octet-stream",
                Content = payload
            });

            var pass = result.IsSuccessWithData
                && result.Data.Content.SequenceEqual(payload)
                && result.Data.Sha256 == sha;

            Pass(cat, "msgpack bytes upload (single msgpack body)", pass,
                $"id={result.Data?.Id} sha={result.Data?.Sha256[..Math.Min(12, result.Data?.Sha256.Length ?? 0)]}... status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "msgpack stream upload (ContractFile -> multipart)", async () =>
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("MessagePack stream upload payload");
            using var ms = new MemoryStream(payload);
            var sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload));

            var result = await Contracts.ProcessAsync(new MessagePackStreamUploadContract
            {
                FileName = "typed-stream.bin",
                MediaType = "application/octet-stream",
                Content = new ContractFile(ms, "typed-stream.bin", "application/octet-stream", payload.Length)
            });

            var pass = result.IsSuccessWithData
                && result.Data.Content.SequenceEqual(payload)
                && result.Data.FileName == "typed-stream.bin"
                && result.Data.Sha256 == sha;

            Pass(cat, "msgpack stream upload (ContractFile -> multipart)", pass,
                $"id={result.Data?.Id} name={result.Data?.FileName} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "msgpack byte[] echo (msgpack envelope on byte[] return)", async () =>
        {
            var payload = new byte[777];
            Random.Shared.NextBytes(payload);
            var sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload));

            var result = await Contracts.ProcessAsync(new TestMsgpackBytesEchoContract { Payload = payload });

            var pass = result.IsSuccessWithData && result.Data.SequenceEqual(payload);

            Pass(cat, "msgpack byte[] echo (msgpack envelope on byte[] return)", pass,
                $"len={result.Data?.Length} shaOK={result.Data is not null && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(result.Data)) == sha}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunFilesAsync()
    {
        var cat = new CategoryResult("Files Upload/Download (typed)");

        await RunCaseAsync(cat, "upload byte[] -> FileResult", async () =>
        {
            var payload = new byte[1536];
            Random.Shared.NextBytes(payload);
            var sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload));

            var result = await Contracts.ProcessAsync(new FileUploadBytesContract
            {
                FileName = "typed-bytes.bin",
                MediaType = "text/plain",
                Content = payload
            });

            var pass = result.IsSuccessWithData
                && result.Data.Id > 0
                && result.Data.Sha256 == sha
                && result.Data.Length == payload.Length;

            Pass(cat, "upload byte[] -> FileResult", pass, $"id={result.Data?.Id} status={result.StatusCode}");

            if (!pass || result.Data is null)
                return;

            var bytesResult = await Contracts.ProcessAsync(new FileDownloadBytesContract { Id = result.Data.Id });
            var bytesPass = bytesResult.IsSuccessWithData && bytesResult.Data.SequenceEqual(payload);
            Pass(cat, "download byte[] round trip", bytesPass, $"len={bytesResult.Data?.Length} status={bytesResult.StatusCode}");

            var streamResult = await Contracts.ProcessAsync(new FileDownloadStreamContract { Id = result.Data.Id });
            await using (streamResult)
            {
                using var ms = new MemoryStream();
                if (streamResult.Data is not null)
                    await streamResult.Data.CopyToAsync(ms);

                var streamPass = streamResult.IsSuccessWithData && ms.ToArray().SequenceEqual(payload);
                Pass(cat, "download stream round trip", streamPass, $"len={ms.Length} status={streamResult.StatusCode}");
            }
        });

        await RunCaseAsync(cat, "upload stream (ContractFile) -> FileResult", async () =>
        {
            var payload = System.Text.Encoding.UTF8.GetBytes("typed stream upload file content");
            using var ms = new MemoryStream(payload);
            var sha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload));

            var result = await Contracts.ProcessAsync(new FileUploadStreamContract
            {
                FileName = "typed-stream.txt",
                Content = new ContractFile(ms, "typed-stream.txt", "text/plain", payload.Length)
            });

            var pass = result.IsSuccessWithData
                && result.Data.Sha256 == sha
                && result.Data.FileName == "typed-stream.txt";

            Pass(cat, "upload stream (ContractFile) -> FileResult", pass, $"id={result.Data?.Id} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "download missing file -> 404 envelope", async () =>
        {
            var result = await Contracts.ProcessAsync(new FileDownloadBytesContract { Id = 999999 });

            var pass = !result.IsSuccess && result.StatusCode == 404 && result.ErrorMessage!.Contains("not found");

            Pass(cat, "download missing file -> 404 envelope", pass,
                $"status={result.StatusCode} err={result.ErrorMessage}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunContractFileAsync()
    {
        var cat = new CategoryResult("ContractFile download (headers + reassembly)");
        using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };

        var payload = new byte[2400];
        for (var i = 0; i < payload.Length; i++)
            payload[i] = (byte)(i % 256);
        const string fileName = "réport-2026.pdf";
        const string expectedDisposition = """attachment; filename="report-2026.pdf"; filename*=UTF-8''r%C3%A9port-2026.pdf""";

        var upload = await Contracts.ProcessAsync(new FileUploadBytesContract
        {
            FileName = fileName,
            MediaType = "application/pdf",
            Content = payload
        });

        await RunCaseAsync(cat, "typed: ContractFile reassembled from headers", async () =>
        {
            var result = await Contracts.ProcessAsync(new FileDownloadFileContract { Id = upload.Data?.Id ?? -1 });
            await using var _ = result;

            using var ms = new MemoryStream();
            if (result.Data is not null)
                await result.Data.Stream.CopyToAsync(ms);

            var pass = result.IsSuccessWithData
                && result.Data!.FileName == fileName
                && result.Data.MediaType == "application/pdf"
                && result.Data.Length == 2400
                && ms.ToArray().SequenceEqual(payload);

            Pass(cat, "typed: ContractFile reassembled from headers", pass,
                $"name={result.Data?.FileName} mt={result.Data?.MediaType} len={result.Data?.Length} bytes={ms.Length} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "raw: 200 + Content-Type/Length/Disposition + exact bytes", async () =>
        {
            using var response = await client.GetAsync($"/api/files/{upload.Data?.Id ?? -1}/file");
            var disposition = response.Content.Headers.TryGetValues("Content-Disposition", out var dv) ? string.Join(", ", dv) : null;
            var body = await response.Content.ReadAsByteArrayAsync();

            var pass = (int)response.StatusCode == 200
                && response.Content.Headers.ContentType?.ToString() == "application/pdf"
                && response.Content.Headers.ContentLength == 2400
                && disposition == expectedDisposition
                && body.SequenceEqual(payload);

            Pass(cat, "raw: 200 + Content-Type/Length/Disposition + exact bytes", pass,
                $"status={(int)response.StatusCode} ct={response.Content.Headers.ContentType} cl={response.Content.Headers.ContentLength} cd={disposition} bytes={body.Length}");
        });

        await RunCaseAsync(cat, "typed: missing file -> 404 envelope without data", async () =>
        {
            var result = await Contracts.ProcessAsync(new FileDownloadFileContract { Id = 999999 });
            await using var _ = result;

            var pass = !result.IsSuccess
                && result.StatusCode == 404
                && result.ErrorMessage!.Contains("not found")
                && result.Data is null;

            Pass(cat, "typed: missing file -> 404 envelope without data", pass,
                $"status={result.StatusCode} err={result.ErrorMessage} data={(result.Data is null ? "null" : "present")}");
        });

        await RunCaseWireAsync(cat, "raw: missing file -> json envelope, no disposition", async () =>
        {
            using var response = await client.GetAsync("/api/files/999999/file");
            var disposition = response.Content.Headers.TryGetValues("Content-Disposition", out var dv) ? string.Join(", ", dv) : null;
            var body = await response.Content.ReadAsByteArrayAsync();
            var (status, error, data) = ParseJsonEnvelope(body);

            var pass = (int)response.StatusCode == 404
                && response.Content.Headers.ContentType?.ToString() == "application/json; charset=utf-8"
                && disposition is null
                && status == 404
                && error is not null && error.Contains("not found")
                && data is null;

            Pass(cat, "raw: missing file -> json envelope, no disposition", pass,
                $"status={(int)response.StatusCode} envelope={status} err={error} cd={disposition}");
        });

        await RunCaseAsync(cat, "typed: msgpack envelope 404 reassembled without data", async () =>
        {
            var result = await Contracts.ProcessAsync(new FileDownloadMsgpackFileContract { Id = 999999 });
            await using var _ = result;

            var pass = !result.IsSuccess
                && result.StatusCode == 404
                && result.ErrorMessage!.Contains("not found")
                && result.Data is null;

            Pass(cat, "typed: msgpack envelope 404 reassembled without data", pass,
                $"status={result.StatusCode} err={result.ErrorMessage}");
        });

        await RunCaseWireAsync(cat, "raw: msgpack envelope 404 (application/x-msgpack)", async () =>
        {
            using var response = await client.GetAsync("/api/files/999999/msgpack-file");
            var disposition = response.Content.Headers.TryGetValues("Content-Disposition", out var dv) ? string.Join(", ", dv) : null;
            var body = await response.Content.ReadAsByteArrayAsync();

            await using var ms = new MemoryStream(body);
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<ContractFile>>(ms);

            var pass = (int)response.StatusCode == 404
                && response.Content.Headers.ContentType?.ToString() == "application/x-msgpack"
                && disposition is null
                && envelope is { StatusCode: 404, Data: null }
                && envelope.ErrorMessage is not null && envelope.ErrorMessage.Contains("not found");

            Pass(cat, "raw: msgpack envelope 404 (application/x-msgpack)", pass,
                $"status={(int)response.StatusCode} ct={response.Content.Headers.ContentType} envelope={envelope?.StatusCode} err={envelope?.ErrorMessage}");
        });

        await RunCaseAsync(cat, "typed: error + file -> multipart reassembled", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetFileWithErrorContract());
            await using var _ = result;

            using var ms = new MemoryStream();
            if (result.Data is not null)
                await result.Data.Stream.CopyToAsync(ms);

            var pass = result.StatusCode == 200
                && result.ErrorMessage is not null
                && result.Data is not null
                && result.Data!.FileName == fileName
                && result.Data.MediaType == "application/pdf"
                && result.Data.Length == 2400
                && ms.ToArray().SequenceEqual(payload);

            Pass(cat, "typed: error + file -> multipart reassembled", pass,
                $"status={result.StatusCode} err={result.ErrorMessage} name={result.Data?.FileName} bytes={ms.Length}");
        });

        await RunCaseWireAsync(cat, "raw: multipart envelope + attachment part with headers", async () =>
        {
            using var response = await client.GetAsync("/api/test/file-with-error");
            var raw = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "";

            var rawText = Encoding.UTF8.GetString(raw);
            var hasEnvelopePartHeader = rawText.Contains("Content-Disposition: form-data; name=\"envTestGetFileWithErrorContract\"");
            var hasFilePartHeader = rawText.Contains(expectedDisposition)
                && rawText.Contains("Content-Type: application/pdf")
                && rawText.Contains("Content-Length: 2400");

            ContractResult<ContractFile>? envelope = null;
            ContractFile? file = null;
            await using var body = new MemoryStream(raw);
            await foreach (var part in ContractMultipart.ParseAsync(body, contentType))
            {
                try
                {
                    if (part.DispositionType?.Equals("attachment", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        using var ms = new MemoryStream();
                        await part.Stream.CopyToAsync(ms);
                        file = new ContractFile(ms, part.FileName ?? "file", part.ContentType ?? ContractHeaders.OctetStreamMediaType, ms.Length);
                    }
                    else if (ContractHeaders.IsJson(part.ContentType))
                        envelope = await JsonSerializer.DeserializeAsync<ContractResult<ContractFile>>(part.Stream);
                }
                finally
                {
                    await part.DisposeAsync();
                }
            }

            var pass = (int)response.StatusCode == 200
                && contentType.StartsWith("multipart/related; boundary=")
                && hasEnvelopePartHeader
                && hasFilePartHeader
                && envelope is { StatusCode: 200, Data: null }
                && envelope.ErrorMessage is not null
                && file is not null
                && file.FileName == fileName
                && file.MediaType == "application/pdf"
                && file.Length == 2400
                && ((MemoryStream)file.Stream).ToArray().SequenceEqual(payload);

            Pass(cat, "raw: multipart envelope + attachment part with headers", pass,
                $"ct={contentType} envelope={envelope?.StatusCode}/{envelope?.ErrorMessage} file={file?.FileName}/{file?.Length}");
        });

        await RunCaseAsync(cat, "typed: compressed ContractFile reassembled (lz4)", async () =>
        {
            await using var result = await Contracts.ProcessAsync(new FileDownloadCompressedFileContract { Id = upload.Data?.Id ?? -1 });

            using var ms = new MemoryStream();
            if (result.Data is not null)
                await result.Data.Stream.CopyToAsync(ms);

            var pass = result.IsSuccessWithData
                && result.Data!.FileName == fileName
                && result.Data.MediaType == "application/pdf"
                && result.Data.Length == 2400
                && result.Data.Length == payload.Length
                && ms.ToArray().SequenceEqual(payload);

            Pass(cat, "typed: compressed ContractFile reassembled (lz4)", pass,
                $"name={result.Data?.FileName} mt={result.Data?.MediaType} len={result.Data?.Length} bytes={ms.Length} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "raw: compressed file -> lz4 header + disposition, chunked body", async () =>
        {
            using var response = await client.GetAsync($"/api/files/{upload.Data?.Id ?? -1}/compressed-file");
            var disposition = response.Content.Headers.TryGetValues("Content-Disposition", out var dv) ? string.Join(", ", dv) : null;
            var lz4 = ContractHeaders.IsLz4Response(response);
            var body = await response.Content.ReadAsByteArrayAsync();

            using var decompressedMs = new MemoryStream();
            await using (var decoder = LZ4Stream.Decode(new MemoryStream(body)))
                await decoder.CopyToAsync(decompressedMs);

            var pass = (int)response.StatusCode == 200
                && lz4
                && response.Content.Headers.ContentType?.ToString() == "application/pdf"
                && disposition == expectedDisposition
                && !body.SequenceEqual(payload)
                && decompressedMs.ToArray().SequenceEqual(payload)
                && ContractHeaders.GetXOriginalSize(response) == 2400;

            Pass(cat, "raw: compressed file -> lz4 header + disposition, chunked body", pass,
                $"status={(int)response.StatusCode} lz4={lz4} ct={response.Content.Headers.ContentType} compressed={body.Length} decompressed={decompressedMs.Length} cd={disposition}");
        });

        await RunCaseAsync(cat, "typed: compressed 404 -> lz4 json envelope without data", async () =>
        {
            await using var result = await Contracts.ProcessAsync(new FileDownloadCompressedFileContract { Id = 999999 });

            var pass = !result.IsSuccess
                && result.StatusCode == 404
                && result.ErrorMessage!.Contains("not found")
                && result.Data is null;

            Pass(cat, "typed: compressed 404 -> lz4 json envelope without data", pass,
                $"status={result.StatusCode} err={result.ErrorMessage} data={(result.Data is null ? "null" : "present")}");
        });

        await RunCaseWireAsync(cat, "raw: compressed 404 -> lz4 json envelope", async () =>
        {
            using var response = await client.GetAsync("/api/files/999999/compressed-file");
            var disposition = response.Content.Headers.TryGetValues("Content-Disposition", out var dv) ? string.Join(", ", dv) : null;
            var lz4 = ContractHeaders.IsLz4Response(response);
            var body = await response.Content.ReadAsByteArrayAsync();

            using var decompressedMs = new MemoryStream();
            await using (var decoder = LZ4Stream.Decode(new MemoryStream(body)))
                await decoder.CopyToAsync(decompressedMs);

            var (status, error, data) = ParseJsonEnvelope(decompressedMs.ToArray());

            var pass = (int)response.StatusCode == 404
                && lz4
                && response.Content.Headers.ContentType?.ToString() == "application/json; charset=utf-8"
                && disposition is null
                && status == 404
                && error is not null && error.Contains("not found")
                && data is null;

            Pass(cat, "raw: compressed 404 -> lz4 json envelope", pass,
                $"status={(int)response.StatusCode} lz4={lz4} ct={response.Content.Headers.ContentType} envelope={status} err={error} cd={disposition}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunContractFileDisposalAsync()
    {
        var cat = new CategoryResult("ContractFile disposal (leaveOpen)");

        await RunCaseAsync(cat, "leaveOpen: true -> stream stays open after DisposeAsync", async () =>
        {
            var payload = new byte[] { 1, 2, 3, 4 };
            var ms = new MemoryStream(payload, writable: false);

            var file = new ContractFile(ms, "kept-open.bin", "application/octet-stream", payload.Length, leaveOpen: true);
            await file.DisposeAsync();

            var pass = file.LeaveOpen
                && ms.CanRead
                && ms.ReadByte() == payload[0];

            Pass(cat, "leaveOpen: true -> stream stays open after DisposeAsync", pass,
                $"leaveOpen={file.LeaveOpen} canRead={ms.CanRead}");

            ms.Dispose();
        });

        await RunCaseAsync(cat, "leaveOpen: false (default) -> stream disposed with DisposeAsync", async () =>
        {
            var payload = new byte[] { 1, 2, 3, 4 };
            var ms = new MemoryStream(payload, writable: false);

            var file = new ContractFile(ms, "owned.bin", "application/octet-stream", payload.Length);
            await file.DisposeAsync();

            var pass = !file.LeaveOpen
                && !ms.CanRead;

            Pass(cat, "leaveOpen: false (default) -> stream disposed with DisposeAsync", pass,
                $"leaveOpen={file.LeaveOpen} canRead={ms.CanRead}");
        });

        await RunCaseAsync(cat, "sync Dispose: stream disposed when leaveOpen is false", async () =>
        {
            var payload = new byte[] { 1, 2, 3, 4 };
            var ms = new MemoryStream(payload, writable: false);

            using (var file = new ContractFile(ms, "sync.bin", "application/octet-stream", payload.Length))
            {
                if (!file.LeaveOpen)
                    _ = file.Stream;
            }

            var pass = !ms.CanRead;

            Pass(cat, "sync Dispose: stream disposed when leaveOpen is false", pass,
                $"canRead={ms.CanRead}");
        });

        await RunCaseAsync(cat, "typed: disposing the result closes the underlying stream", async () =>
        {
            var upload = await Contracts.ProcessAsync(new FileUploadBytesContract
            {
                FileName = "disposal-check.pdf",
                MediaType = "application/pdf",
                Content = new byte[] { 9, 8, 7, 6 }
            });

            var result = await Contracts.ProcessAsync(new FileDownloadFileContract { Id = upload.Data?.Id ?? -1 });
            Stream? stream = result.Data?.Stream;
            var hasData = result.Data is not null;

            await result.DisposeAsync();

            var readThrew = false;
            if (stream is not null)
            {
                try
                {
                    _ = await stream.ReadAsync(new byte[1]);
                }
                catch (ObjectDisposedException)
                {
                    readThrew = true;
                }
                catch (IOException)
                {
                    readThrew = true;
                }
            }

            var pass = hasData && readThrew;

            Pass(cat, "typed: disposing the result closes the underlying stream", pass,
                $"hasData={hasData} readAfterDisposeThrew={readThrew}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunGeneratorEdgeCasesAsync()
    {
        var cat = new CategoryResult("Generator Edge Cases (typed)");

        await RunCaseWireAsync(cat, "getter-only props bound via UnsafeAccessor (raw query)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/get-getter-only?Echo=getter-only&Repeat=3");
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Echo=getter-only;Repeat=3";

            Pass(cat, "getter-only props bound via UnsafeAccessor (raw query)", pass,
                $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())} status={status}");
        });

        await RunCaseAsync(cat, "init-only route param (PUT)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestPutInitRouteParamContract { Id = 5, Message = "init-route" });

            var pass = result.IsSuccessWithData && result.Data.Message == "Id=5;Message=init-route";

            Pass(cat, "init-only route param (PUT)", pass, $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "endpoint attributes flow to endpoint metadata", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestEndpointAttributesContract());

            var pass = result.IsSuccessWithData && result.Data.Message == "endpoint attributes ok";

            Pass(cat, "endpoint attributes flow to endpoint metadata", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "disposable response carries data", async () =>
        {
            await using var result = await Contracts.ProcessAsync(new TestGetDisposableResponseContract());

            var pass = result.IsSuccessWithData && result.Data.Content is { Length: > 0 };

            Pass(cat, "disposable response carries data", pass,
                $"len={result.Data?.Content.Length ?? 0} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "query params with JsonPropertyName keys", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetQueryParamsContract { Filter = "wire", Page = 9, Enabled = false });

            var pass = result.IsSuccessWithData && result.Data.Message == "Filter=wire;Page=9;Enabled=False";

            Pass(cat, "query params with JsonPropertyName keys", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "inherited contract properties", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestPostInheritedContract { Title = "base", Count = 41, Suffix = "derived" });

            var pass = result.IsSuccessWithData && result.Data.Message == "base#41#derived";

            Pass(cat, "inherited contract properties", pass, $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "string route param escaping", async () =>
        {
            var name = "a b&c=d?e 100%! 日本語 テスト";
            var result = await Contracts.ProcessAsync(new TestGetRouteStringContract { Name = name });

            var pass = result.IsSuccessWithData && result.Data.Message == name;

            Pass(cat, "string route param escaping", pass,
                $"msg={(result.Data?.Message == name ? "round trip OK" : result.Data?.Message)} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "enum route param: valid name -> 200 echo (static Enum.TryParse)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetRouteEnumContract { Level = TestRouteLevel.High });

            var pass = result.IsSuccessWithData && result.Data.Message == "Level=High";

            Pass(cat, "enum route param: valid name -> 200 echo (static Enum.TryParse)", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "enum route param: bad name -> 400 contract envelope (no type/name leak)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/enum/NotARealOne");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null;

            Pass(cat, "enum route param: bad name -> 400 contract envelope (no type/name leak)", pass,
                $"status={status} error={error}");
        });

        await RunCaseWireAsync(cat, "enum route param: numeric out-of-range -> 400 (strict Enum.IsDefined)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/enum/999");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null;

            Pass(cat, "enum route param: numeric out-of-range -> 400 (strict Enum.IsDefined)", pass,
                $"status={status} error={error}");
        });

        await RunCaseWireAsync(cat, "enum query param: undefined value -> 400 (strict Enum.IsDefined)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/msgpack-compressed/1?Scope=999");
            var materialized = await MaterializeResponseAsync(response);

            var pass = materialized.Status == 400;

            Pass(cat, "enum query param: undefined value -> 400 (strict Enum.IsDefined)", pass,
                $"status={materialized.Status} ct={materialized.ContentType}");
        });

        await RunCaseAsync(cat, "nullable route param: has value -> 200 echo (static TryParse + nullable)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetRouteNullableContract { Value = 77 });

            var pass = result.IsSuccessWithData && result.Data.Message == "Value=77";

            Pass(cat, "nullable route param: has value -> 200 echo (static TryParse + nullable)", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "nullable route param: bad value -> 400 contract envelope (no type/name leak)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/route-nullable/x");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null;

            Pass(cat, "nullable route param: bad value -> 400 contract envelope (no type/name leak)", pass,
                $"status={status} error={error}");
        });

        // A route segment is positionally always present, so "absent -> null" is untestable
        // over URL (it's a query-param concept and is covered by the query-param regressions).
        // The reachable matrix for a nullable route param is: valid -> 200 echo, invalid -> 400.
        // The typed-Service case above may run in-process on the SSR/Server pass, so prove the
        // nullable TryParse binding over the wire here with a raw client.
        await RunCaseWireAsync(cat, "nullable route param: has value -> 200 echo (raw HTTP over the wire)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/route-nullable/77");
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Value=77";

            Pass(cat, "nullable route param: has value -> 200 echo (raw HTTP over the wire)", pass,
                $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())} status={status}");
        });

        await RunCaseAsync(cat, "DELETE with query params + custom names", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestDeleteQueryContract { Filter = "del", Page = 4 });

            var pass = result.IsSuccessWithData && result.Data.Message == "Filter=del;Page=4";

            Pass(cat, "DELETE with query params + custom names", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunQueryOptionalityAsync()
    {
        var cat = new CategoryResult("Query Optionality (GET/DELETE)");

        await RunCaseWireAsync(cat, "GET optional: empty query -> 200 with defaults", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/query-optional");
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Text=;Count=null;Flag=False";

            Pass(cat, "GET optional: empty query -> 200 with defaults", pass,
                $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())} status={status}");
        });

        await RunCaseAsync(cat, "GET optional: typed client sends no query when everything is default", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetQueryOptionalContract());

            var pass = result.IsSuccessWithData && result.Data.Message == "Text=;Count=null;Flag=False";

            Pass(cat, "GET optional: typed client sends no query when everything is default", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "GET optional: values bound when provided", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetQueryOptionalContract { Text = "hi", Count = 7, Flag = true });

            var pass = result.IsSuccessWithData && result.Data.Message == "Text=hi;Count=7;Flag=True";

            Pass(cat, "GET optional: values bound when provided", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "DELETE optional: empty query -> 200 with defaults", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.DeleteAsync("/api/test/query-optional-delete");
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Tag=;Limit=null";

            Pass(cat, "DELETE optional: empty query -> 200 with defaults", pass,
                $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())} status={status}");
        });

        await RunCaseAsync(cat, "DELETE optional: values bound when provided", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestDeleteQueryOptionalContract { Tag = "t", Limit = 3 });

            var pass = result.IsSuccessWithData && result.Data.Message == "Tag=t;Limit=3";

            Pass(cat, "DELETE optional: values bound when provided", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "GET required: missing query -> 400 contract envelope", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/query-required");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "Required query parameter \"Name\" is missing."
                && data is null
                && (materialized.ContentType is null || materialized.ContentType.StartsWith("application/json"));

            Pass(cat, "GET required: missing query -> 400 contract envelope", pass,
                $"status={status} error={error} ct={materialized.ContentType}");
        });

        await RunCaseWireAsync(cat, "GET required: custom-named one missing -> envelope mentions its query key", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/query-required?Name=abc");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, _) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "Required query parameter \"note_text\" is missing.";

            Pass(cat, "GET required: custom-named one missing -> envelope mentions its query key", pass,
                $"status={status} error={error}");
        });

        await RunCaseAsync(cat, "GET required: all provided -> 200 echo", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetQueryRequiredContract { Name = "abc", Note = "xyz", Extra = "e" });

            var pass = result.IsSuccessWithData && result.Data.Message == "Name=abc;Note=xyz;Extra=e";

            Pass(cat, "GET required: all provided -> 200 echo", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "GET required: only optional given on the wire -> 400 envelope", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/query-required?Extra=e");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "Required query parameter \"Name\" is missing."
                && data is null;

            Pass(cat, "GET required: only optional given on the wire -> 400 envelope", pass,
                $"status={status} error={error}");
        });

        await RunCaseWireAsync(cat, "DELETE required: missing query -> 400 contract envelope", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.DeleteAsync("/api/test/query-required-delete");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "Required query parameter \"Id\" is missing."
                && data is null;

            Pass(cat, "DELETE required: missing query -> 400 contract envelope", pass,
                $"status={status} error={error}");
        });

        await RunCaseAsync(cat, "DELETE required: provided -> 200 echo", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestDeleteQueryRequiredContract { Id = "42", Reason = "done" });

            var pass = result.IsSuccessWithData && result.Data.Message == "Id=42;Reason=done";

            Pass(cat, "DELETE required: provided -> 200 echo", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseWireAsync(cat, "GET route: bad int -> 400 contract envelope", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/abc");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null
                && (materialized.ContentType is null || materialized.ContentType.StartsWith("application/json"));

            Pass(cat, "GET route: bad int -> 400 contract envelope (no type/name leak)", pass,
                $"status={status} error={error} ct={materialized.ContentType}");
        });

        await RunCaseWireAsync(cat, "GET route: good int -> handler echoes Id*10 as status", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/20");
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, _) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200 && status == 200;

            Pass(cat, "GET route: good int -> handler echoes Id*10 as status", pass,
                $"http={materialized.Status} body-status={status}");
        });

        await RunCaseWireAsync(cat, "PUT route: bad int -> 400 contract envelope", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.PutAsync(
                "/api/test/update/abc",
                new StringContent("{\"Number\":5,\"Message\":\"m\"}", Encoding.UTF8, "application/json"));
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null;

            Pass(cat, "PUT route: bad int -> 400 contract envelope (no type/name leak)", pass,
                $"status={status} error={error}");
        });

        await RunCaseWireAsync(cat, "PUT route: good int -> 200 echo (route+body)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.PutAsync(
                "/api/test/update/3",
                new StringContent("{\"Number\":7}", Encoding.UTF8, "application/json"));
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "21";

            Pass(cat, "PUT route: good int -> 200 echo (route+body)", pass,
                $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())} status={status}");
        });

        await RunCaseWireAsync(cat, "GET route: overflow int -> 400 route envelope (not framework 500/400)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/99999999999999");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null;

            Pass(cat, "GET route: overflow int -> 400 route envelope (not framework 500/400)", pass,
                $"status={status} error={error}");
        });

        await RunCaseWireAsync(cat, "GET query: bad int -> 400 generic envelope (not framework 500/HTML 400)", async () =>
        {
            using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };
            using var response = await client.GetAsync("/api/test/query/7?Filter=x&Page=Hello,%20world!");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "The request contains an invalid value."
                && data is null;

            Pass(cat, "GET query: bad int -> 400 generic envelope (not framework 500/HTML 400)", pass,
                $"status={status} error={error}");
        });

        await RunCaseAsync(cat, "GET query: valid int -> 200 echo (regression)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetRouteQueryParamContract { Id = 7, Filter = "x", Page = 42 });

            var pass = result.IsSuccessWithData && result.Data.Message == "7x42";

            Pass(cat, "GET query: valid int -> 200 echo (regression)", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "GET query: absent int -> 200 with null (optional, no 400)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetRouteQueryParamContract { Id = 7, Filter = "x" });

            var pass = result.IsSuccessWithData && result.Data.Message == "7x";

            Pass(cat, "GET query: absent int -> 200 with null (optional, no 400)", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunCompressedAsync()
    {
        var cat = new CategoryResult("Compressed GET (typed)");

        await RunCaseAsync(cat, "compressed json envelope (LZ4Level.L03_HC)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetCompressedEnvelopeContract { Echo = "typed-lz4" });

            var pass = result.IsSuccessWithData && result.Data.Message == "Compressed envelope echo: typed-lz4";

            Pass(cat, "compressed json envelope (LZ4Level.L03_HC)", pass,
                $"msg={result.Data?.Message} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "compressed stream response", async () =>
        {
            await using var result = await Contracts.ProcessAsync(new TestGetCompressedStreamContract());

            using var ms = new MemoryStream();
            if (result.Data is not null)
                await result.Data.CopyToAsync(ms);
            var data = ms.ToArray();

            var pass = result.IsSuccessWithData
                && data.Length == 256
                && Enumerable.Range(0, 256).All(i => data[i] == (byte)i);

            Pass(cat, "compressed stream response", pass, $"len={data.Length} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "compressed byte[] response", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestGetCompressedBytesContract());

            var pass = result.IsSuccessWithData
                && result.Data.Length == 512
                && Enumerable.Range(0, 512).All(i => result.Data[i] == (byte)(i % 256));

            Pass(cat, "compressed byte[] response", pass, $"len={result.Data?.Length} status={result.StatusCode}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunTodoCrudAsync()
    {
        var cat = new CategoryResult("Todo CRUD (typed)");
        var marker = $"test-crud-{Random.Shared.Next(100000)}";

        await RunCaseAsync(cat, "create todo", async () =>
        {
            var result = await Contracts.ProcessAsync(new TodoCreateContract { Title = marker, IsDone = false });

            var pass = result.IsSuccessWithData && result.Data.Id > 0 && result.Data.Title == marker;

            Pass(cat, "create todo", pass, $"id={result.Data?.Id} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "get by id + update", async () =>
        {
            var all = await Contracts.ProcessAsync(new TodoGetAllContract());
            var created = all.Data?.FirstOrDefault(t => t.Title == marker);
            if (created is null)
            {
                Pass(cat, "get by id + update", false, "created todo not found in get-all");
                return;
            }

            var byId = await Contracts.ProcessAsync(new TodoGetByIdContract { Id = created.Id });
            var byIdOk = byId.IsSuccessWithData && byId.Data.Title == marker;

            var updated = await Contracts.ProcessAsync(new TodoUpdateContract { Id = created.Id, Title = marker + "-updated", IsDone = true });
            var updateOk = updated.IsSuccessWithData && updated.Data.Title == marker + "-updated" && updated.Data.IsDone;

            var allAfter = await Contracts.ProcessAsync(new TodoGetAllContract());
            var allOk = allAfter.Data?.Any(t => t.Id == created.Id && t.IsDone) == true;

            Pass(cat, "get by id + update", byIdOk && updateOk && allOk,
                $"byId={byIdOk} update={updateOk} getAll={allOk}");
        });

        await RunCaseAsync(cat, "delete done (array response)", async () =>
        {
            var all = await Contracts.ProcessAsync(new TodoGetAllContract());
            var doneBefore = all.Data?.Where(t => t.IsDone).Select(t => t.Id).ToHashSet() ?? new HashSet<int>();

            if (doneBefore.Count == 0)
            {
                Pass(cat, "delete done (array response)", false, "no done todos to delete");
                return;
            }

            var deleted = await Contracts.ProcessAsync(new TodoDeleteDoneContract());

            var pass = deleted.IsSuccessWithData
                && deleted.Data.Length == doneBefore.Count
                && deleted.Data.All(t => doneBefore.Contains(t.Id));

            var allAfter = await Contracts.ProcessAsync(new TodoGetAllContract());
            pass = pass && allAfter.Data?.All(t => !t.IsDone) == true;

            Pass(cat, "delete done (array response)", pass,
                $"deleted={deleted.Data?.Length} status={deleted.StatusCode}");
        });

        await RunCaseAsync(cat, "delete by id + 404 afterwards", async () =>
        {
            var created = await Contracts.ProcessAsync(new TodoCreateContract { Title = marker + "-doomed", IsDone = false });
            if (!created.IsSuccessWithData)
            {
                Pass(cat, "delete by id + 404 afterwards", false, $"create failed: {created.StatusCode}");
                return;
            }

            var deleted = await Contracts.ProcessAsync(new TodoDeleteContract { Id = created.Data.Id });
            var deleteOk = deleted.IsSuccess;

            var after = await Contracts.ProcessAsync(new TodoGetByIdContract { Id = created.Data.Id });
            var goneOk = after.StatusCode == 404 && after.ErrorMessage!.Contains("not found");

            Pass(cat, "delete by id + 404 afterwards", deleteOk && goneOk,
                $"delete={deleteOk} after={after.StatusCode}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunWeatherAsync()
    {
        var cat = new CategoryResult("Weather (typed)");

        await RunCaseAsync(cat, "weather get all with query params", async () =>
        {
            var result = await Contracts.ProcessAsync(new WeatherGetContract { City = "TestCity", WithSummary = true });

            var pass = result.IsSuccessWithData
                && result.Data.Length == 5
                && result.Data.All(w => w.City == "TestCity" && w.Summary is not null);

            Pass(cat, "weather get all with query params", pass,
                $"count={result.Data?.Length} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "weather get by id", async () =>
        {
            var result = await Contracts.ProcessAsync(new WeatherGetByIdContract { Id = 1 });

            var pass = result.IsSuccessWithData && result.Data.TemperatureC is >= -20 and < 55;

            Pass(cat, "weather get by id", pass, $"temp={result.Data?.TemperatureC} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "weather get by id out of range -> 404", async () =>
        {
            var result = await Contracts.ProcessAsync(new WeatherGetByIdContract { Id = 42 });

            var pass = result.StatusCode == 404 && result.ErrorMessage!.Contains("not found");

            Pass(cat, "weather get by id out of range -> 404", pass,
                $"status={result.StatusCode} err={result.ErrorMessage}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunSseAsync()
    {
        var cat = new CategoryResult("SSE (typed)");

        await RunCaseAsync(cat, "json events: 3 events with id/event/retry round trip", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestSseJsonContract { Count = 3 });

            var items = new List<SseItem<SimpleTestResponse?>>();
            if (result.Data is not null)
                await foreach (var item in result.Data)
                    items.Add(item);

            var pass = result.IsSuccess
                && items.Count == 3
                && items[0].EventId == "0" && items[0].EventType == "tick" && items[0].ReconnectionInterval == TimeSpan.FromSeconds(1)
                && items[1].EventId == "1" && items[1].ReconnectionInterval is null
                && items[2].EventId == "2" && items[2].EventType == "tick"
                && items[0].Data?.Message == "Event 0" && items[1].Data?.Message == "Event 1" && items[2].Data?.Message == "Event 2";

            Pass(cat, "json events: 3 events with id/event/retry round trip", pass,
                $"items={items.Count} ids=[{string.Join(",", items.Select(x => x.EventId))}] retry0={(items.Count > 0 ? items[0].ReconnectionInterval?.TotalMilliseconds : null)}ms status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "compressed json events: base122 + lz4 decoded", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestSseCompressedJsonContract { Count = 3 });

            var items = new List<SseItem<SimpleTestResponse?>>();
            if (result.Data is not null)
                await foreach (var item in result.Data)
                    items.Add(item);

            var pass = result.IsSuccess
                && items.Count == 3
                && items.All(x => x.EventType == "tick" && x.EventId is null && x.ReconnectionInterval is null)
                && items[0].Data?.Message == "Compressed event 0" && items[2].Data?.Message == "Compressed event 2";

            Pass(cat, "compressed json events: base122 + lz4 decoded", pass,
                $"items={items.Count} msg2={(items.Count > 2 ? items[2].Data?.Message : "n/a")} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "msgpack events: base122 + msgpack decoded", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestSseMsgPackContract { Count = 2 });

            var items = new List<SseItem<MessagePackWeatherForecast?>>();
            if (result.Data is not null)
                await foreach (var item in result.Data)
                    items.Add(item);

            var pass = result.IsSuccess
                && items.Count == 2
                && items.All(x => x.EventType == "forecast")
                && items[0].Data?.Summary == "Forecast event 0" && items[1].Data?.Summary == "Forecast event 1"
                && items[0].Data?.TemperatureC == 10 && items[1].Data?.TemperatureC == 11
                && items[0].Data?.Date == new DateTime(2026, 1, 1);

            Pass(cat, "msgpack events: base122 + msgpack decoded", pass,
                $"items={items.Count} temp={(items.Count > 0 ? items[0].Data?.TemperatureC : null)} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "compressed msgpack events: base122 + lz4 + msgpack decoded", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestSseMsgPackCompressedContract { Count = 2 });

            var items = new List<SseItem<MessagePackWeatherForecast?>>();
            if (result.Data is not null)
                await foreach (var item in result.Data)
                    items.Add(item);

            var pass = result.IsSuccess
                && items.Count == 2
                && items[0].Data?.Summary == "Compressed forecast event 0" && items[1].Data?.Summary == "Compressed forecast event 1"
                && items[0].Data?.TemperatureC == -10 && items[1].Data?.TemperatureC == -11;

            Pass(cat, "compressed msgpack events: base122 + lz4 + msgpack decoded", pass,
                $"items={items.Count} temp0={(items.Count > 0 ? items[0].Data?.TemperatureC : null)} status={result.StatusCode}");
        });

        await RunCaseAsync(cat, "streaming events arrive over time (3 x ~250ms)", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestSseStreamingContract { Count = 3 });
            var timestamps = new List<DateTimeOffset>();
            var messages = new List<string?>();

            if (result.Data is not null)
            {
                await foreach (var item in result.Data)
                {
                    timestamps.Add(DateTimeOffset.UtcNow);
                    messages.Add(item.Data?.Message);
                }
            }

            var pass = result.IsSuccess
                && timestamps.Count == 3
                && messages[0] == "Streamed event 0"
                && 100 < (timestamps[1] - timestamps[0]).TotalMilliseconds
                && 100 < (timestamps[2] - timestamps[1]).TotalMilliseconds;

            Pass(cat, "streaming events arrive over time (3 x ~250ms)", pass,
                $"count={timestamps.Count} gaps={(timestamps.Count >= 3 ? $"{(timestamps[1] - timestamps[0]).TotalMilliseconds:F0}ms/{(timestamps[2] - timestamps[1]).TotalMilliseconds:F0}ms" : "n/a")}");
        });

        await RunCaseAsync(cat, "handler error -> 400 envelope without events", async () =>
        {
            var result = await Contracts.ProcessAsync(new TestSseJsonContract { Count = -1 });

            var pass = !result.IsSuccess
                && result.StatusCode == 400
                && result.ErrorMessage == "Count must be non-negative."
                && result.Data is null;

            Pass(cat, "handler error -> 400 envelope without events", pass,
                $"status={result.StatusCode} err={result.ErrorMessage} data={(result.Data is null ? "null" : "present")}");
        });

        _categories.Add(cat);
        Recount();
    }

    // Wire-only: raw HTTP checks of the server-sent-events framing, headers and the
    // base122/lz4/msgpack payload encoding. Runs in the WebAssembly pass only, next to
    // the other raw wire tests.
    private async Task RunSseWireFormatsAsync()
    {
        var cat = new CategoryResult("SSE Wire Formats (raw HTTP)");
        using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };

        await RunCaseAsync(cat, "GET json events: exact wire format + sse headers", async () =>
        {
            using var response = await client.GetAsync("/api/test/sse/json?Count=2");
            var body = await response.Content.ReadAsStringAsync();
            var xct = response.Headers.TryGetValues(ContractHeaders.XContentTypeHeader, out var v) ? v.FirstOrDefault() : null;

            var expected =
                "id: 0\nevent: tick\nretry: 1000\ndata: {\"Message\":\"Event 0\"}\n\n" +
                "id: 1\nevent: tick\ndata: {\"Message\":\"Event 1\"}\n\n";

            var pass = (int)response.StatusCode == 200
                && response.Content.Headers.ContentType?.MediaType == ContractHeaders.SseMediaType
                && xct == ContractHeaders.JsonMediaType
                && !ContractHeaders.IsLz4Response(response)
                && body == expected;

            Pass(cat, "GET json events: exact wire format + sse headers", pass,
                $"status={(int)response.StatusCode} ct={response.Content.Headers.ContentType?.MediaType} xct={xct} len={body.Length}");
        });

        await RunCaseAsync(cat, "GET json events: parsed with SseParser", async () =>
        {
            using var response = await client.GetAsync("/api/test/sse/json?Count=3");
            var body = await response.Content.ReadAsByteArrayAsync();

            var items = new List<SseItem<SimpleTestResponse>>();
            var parser = SseParser.Create(new MemoryStream(body), (eventType, data) => JsonSerializer.Deserialize<SimpleTestResponse>(data)!);
            await foreach (var item in parser.EnumerateAsync())
                items.Add(item);

            var pass = items.Count == 3
                && items[0].EventId == "0" && items[0].EventType == "tick" && items[0].ReconnectionInterval == TimeSpan.FromSeconds(1)
                && items[2].Data?.Message == "Event 2";

            Pass(cat, "GET json events: parsed with SseParser", pass,
                $"items={items.Count} msg2={(items.Count > 2 ? items[2].Data?.Message : "n/a")}");
        });

        await RunCaseAsync(cat, "POST compressed json: lz4 header, base122 data decodes to json", async () =>
        {
            using var response = await client.PostAsync("/api/test/sse/compressed-json", JsonContent("""{"Count":2}"""));
            var body = await response.Content.ReadAsStringAsync();
            var xct = response.Headers.TryGetValues(ContractHeaders.XContentTypeHeader, out var v) ? v.FirstOrDefault() : null;

            var events = new List<SseItem<string>>();
            var parser = SseParser.Create(new MemoryStream(Encoding.UTF8.GetBytes(body)));
            await foreach (var item in parser.EnumerateAsync())
                events.Add(item);

            var decoded = new List<SimpleTestResponse>();
            foreach (var e in events)
            {
                var pickled = Kanawanagasaki.Base122.Base122Decoder.DecodeFromUtf8(Encoding.UTF8.GetBytes(e.Data));
                var json = LZ4Pickler.Unpickle(pickled);
                decoded.Add(JsonSerializer.Deserialize<SimpleTestResponse>(json)!);
            }

            var pass = (int)response.StatusCode == 200
                && response.Content.Headers.ContentType?.MediaType == ContractHeaders.SseMediaType
                && xct == ContractHeaders.JsonMediaType
                && ContractHeaders.IsLz4Response(response)
                && !body.Contains("Message")
                && events.Count == 2
                && decoded[0].Message == "Compressed event 0" && decoded[1].Message == "Compressed event 1";

            Pass(cat, "POST compressed json: lz4 header, base122 data decodes to json", pass,
                $"events={events.Count} msg0={(decoded.Count > 0 ? decoded[0].Message : "n/a")} lz4={ContractHeaders.IsLz4Response(response)}");
        });

        await RunCaseAsync(cat, "POST msgpack: base122 data decodes to msgpack", async () =>
        {
            using var response = await client.PostAsync("/api/test/sse/msgpack", JsonContent("""{"Count":2}"""));
            var body = await response.Content.ReadAsStringAsync();
            var xct = response.Headers.TryGetValues(ContractHeaders.XContentTypeHeader, out var v) ? v.FirstOrDefault() : null;

            var events = new List<SseItem<string>>();
            var parser = SseParser.Create(new MemoryStream(Encoding.UTF8.GetBytes(body)));
            await foreach (var item in parser.EnumerateAsync())
                events.Add(item);

            var decoded = new List<MessagePackWeatherForecast>();
            foreach (var e in events)
                decoded.Add(BlazorContractsMessagePack.Deserialize<MessagePackWeatherForecast>(Kanawanagasaki.Base122.Base122Decoder.DecodeFromUtf8(Encoding.UTF8.GetBytes(e.Data)))!);

            var pass = (int)response.StatusCode == 200
                && xct == ContractHeaders.MessagePackMediaType
                && !ContractHeaders.IsLz4Response(response)
                && events.Count == 2
                && events.All(e => e.EventType == "forecast")
                && decoded[0].Summary == "Forecast event 0" && decoded[1].TemperatureC == 11;

            Pass(cat, "POST msgpack: base122 data decodes to msgpack", pass,
                $"events={events.Count} summary0={(decoded.Count > 0 ? decoded[0].Summary : "n/a")} xct={xct}");
        });

        await RunCaseAsync(cat, "POST compressed msgpack: base122 + lz4 + msgpack", async () =>
        {
            using var response = await client.PostAsync("/api/test/sse/msgpack-compressed", JsonContent("""{"Count":2}"""));
            var body = await response.Content.ReadAsStringAsync();
            var xct = response.Headers.TryGetValues(ContractHeaders.XContentTypeHeader, out var v) ? v.FirstOrDefault() : null;

            var events = new List<SseItem<string>>();
            var parser = SseParser.Create(new MemoryStream(Encoding.UTF8.GetBytes(body)));
            await foreach (var item in parser.EnumerateAsync())
                events.Add(item);

            var decoded = new List<MessagePackWeatherForecast>();
            foreach (var e in events)
            {
                var pickled = Kanawanagasaki.Base122.Base122Decoder.DecodeFromUtf8(Encoding.UTF8.GetBytes(e.Data));
                var msgpack = LZ4Pickler.Unpickle(pickled);
                decoded.Add(BlazorContractsMessagePack.Deserialize<MessagePackWeatherForecast>(msgpack)!);
            }

            var pass = (int)response.StatusCode == 200
                && xct == ContractHeaders.MessagePackMediaType
                && ContractHeaders.IsLz4Response(response)
                && events.Count == 2
                && decoded[0].Summary == "Compressed forecast event 0" && decoded[1].TemperatureC == -11;

            Pass(cat, "POST compressed msgpack: base122 + lz4 + msgpack", pass,
                $"events={events.Count} temp1={(decoded.Count > 1 ? decoded[1].TemperatureC : null)} lz4={ContractHeaders.IsLz4Response(response)}");
        });

        await RunCaseAsync(cat, "GET streaming: chunked transfer, events on the wire", async () =>
        {
            using var response = await client.GetAsync("/api/test/sse/streaming?Count=2", HttpCompletionOption.ResponseHeadersRead);
            var xct = response.Headers.TryGetValues(ContractHeaders.XContentTypeHeader, out var v) ? v.FirstOrDefault() : null;

            var events = new List<SseItem<SimpleTestResponse>>();
            var stream = await response.Content.ReadAsStreamAsync();
            var parser = SseParser.Create<SimpleTestResponse>(stream, (eventType, data) => JsonSerializer.Deserialize<SimpleTestResponse>(data)!);
            await foreach (var item in parser.EnumerateAsync())
                events.Add(item);

            var pass = (int)response.StatusCode == 200
                && response.Content.Headers.ContentType?.MediaType == ContractHeaders.SseMediaType
                && xct == ContractHeaders.JsonMediaType
                && events.Count == 2
                && events[0].Data?.Message == "Streamed event 0";

            Pass(cat, "GET streaming: chunked transfer, events on the wire", pass,
                $"events={events.Count} msg0={(events.Count > 0 ? events[0].Data?.Message : "n/a")}");
        });

        await RunCaseAsync(cat, "GET handler error: 400 json envelope, not event-stream", async () =>
        {
            using var response = await client.GetAsync("/api/test/sse/json?Count=-1");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 400
                && status == 400
                && error == "Count must be non-negative."
                && data is null
                && materialized.ContentType == "application/json; charset=utf-8"
                && !ContractHeaders.IsSse(materialized.ContentType);

            Pass(cat, "GET handler error: 400 json envelope, not event-stream", pass,
                $"status={materialized.Status} envelope={status} err={error} ct={materialized.ContentType}");
        });

        _categories.Add(cat);
        Recount();
    }

    async Task TestGetNoData(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"GET /api/test (no data)", null));
            var result = await Contracts.ProcessAsync(new TestGetNoDataContract());

            var pass = result.StatusCode == 204;
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode}"));

            if (pass)
                _logs.Add(new LogEntry($"GET /api/test returned {result.StatusCode}", true));
            else
                _logs.Add(new LogEntry($"GET /api/test FAILED: status={result.StatusCode}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"GET /api/test threw: {ex.Message}", false));
        }
    }

    async Task TestGetSimpleResponse(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"GET /api/test/simple", null));
            var result = await Contracts.ProcessAsync(new TestGetSimpleResponseContract());

            var pass = result.IsSuccessWithData && int.TryParse(result.Data.Message, out var remoteMin) && Math.Abs(DateTimeOffset.Now.Minute - remoteMin) <= 1;
            cat.Items.Add(new CategoryItem(label, pass, pass ? "simple response OK" : $"status={result.StatusCode}, msg={result.Data?.Message ?? "NULL"}"));

            if (result.IsSuccessWithData)
                _logs.Add(new LogEntry($"GET /api/test/simple: msg={result.Data.Message}", true));
            else
                _logs.Add(new LogEntry($"GET /api/test/simple FAILED: status={result.StatusCode}, msg={result.Data?.Message}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"GET /api/test/simple threw: {ex.Message}", false));
        }
    }

    async Task TestGetComplexResponse(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"GET /api/test/complex", null));
            var result = await Contracts.ProcessAsync(new TestGetComplexResponseContract());

            int validationCode = 0;
            var pass = result.IsSuccessWithData && 0 <= (validationCode = ComplexResponse.Validate(result.Data.ProcessedAt.Minute, result.Data));
            cat.Items.Add(new CategoryItem(label, pass, pass ? "PASS" : $"status={result.StatusCode}"));

            if (pass)
                _logs.Add(new LogEntry($"GET /api/test/complex: PASS", true));
            else
                _logs.Add(new LogEntry($"GET /api/test/complex FAILED: status={result.StatusCode}, validationCode={validationCode}, errMsg={result.ErrorMessage}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"GET /api/test/complex threw: {ex.Message}", false));
        }
    }

    async Task TestGetStreamResponse(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"GET /api/test/stream", null));
            await using var result = await Contracts.ProcessAsync(new TestGetStreamResponseContract());

            if (!result.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"GET /api/test/stream FAILED: status={result.StatusCode}", false));
                return;
            }

            using var ms = new MemoryStream();
            await result.Data.CopyToAsync(ms);
            var data = ms.ToArray();
            var pass = data.Length == 256;
            if (pass)
            {
                for (int i = 0; i < 256; i++)
                {
                    if (data[i] != i)
                        pass = false;
                }
            }

            cat.Items.Add(new CategoryItem(label, pass, $"len={data.Length}B"));

            if (pass)
                _logs.Add(new LogEntry($"GET /api/test/stream: VERIFIED {data.Length} bytes", true));
            else
                _logs.Add(new LogEntry($"GET /api/test/stream FAILED: data mismatch len={data.Length}", false));

            await result.Data.DisposeAsync();
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"GET /api/test/stream threw: {ex.Message}", false));
        }
    }

    async Task TestGetRouteParamNoResponse(CategoryResult cat, string label)
    {
        int num = Random.Shared.Next(20, 60);
        try
        {
            _logs.Add(new LogEntry($"GET /api/test/{num}", null));
            var result = await Contracts.ProcessAsync(new TestGetRouteParamNoResponseContract { Id = num });

            var pass = result.StatusCode == num * 10;
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode}"));

            if (pass)
                _logs.Add(new LogEntry($"GET /api/test/{num} returned {result.StatusCode}", true));
            else
                _logs.Add(new LogEntry($"GET /api/test/{num} FAILED: status={result.StatusCode}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"GET /api/test/{num} threw: {ex.Message}", false));
        }
    }

    async Task TestGetRouteQueryParam(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"GET /api/test/query/7?Filter=test&page=1", null));
            var result = await Contracts.ProcessAsync(new TestGetRouteQueryParamContract
            {
                Id = 7,
                Filter = "test",
                Page = 1
            });

            var pass = result.IsSuccessWithData && result.Data.Message == "7test1";
            cat.Items.Add(new CategoryItem(label, pass, $"ok={pass}"));

            if (pass)
                _logs.Add(new LogEntry($"GET route+query SUCCESS: msg={result.Data?.Message ?? "NULL"}", true));
            else
                _logs.Add(new LogEntry($"GET route+query FAILED: status={result.StatusCode}, msg={result.Data?.Message ?? "NULL"}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"GET route+query threw: {ex.Message}", false));
        }
    }

    async Task TestPostNoData(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"POST /api/test/simple-response-no-req", null));
            var result = await Contracts.ProcessAsync(new TestPostNoDataContract());

            var pass = result.IsSuccessWithData;
            cat.Items.Add(new CategoryItem(label, pass, $"ok={pass}"));

            if (pass)
                _logs.Add(new LogEntry($"POST no data: msg=#{result.Data?.Message ?? "NULL"}", true));
            else
                _logs.Add(new LogEntry($"POST no data FAILED: status={result.StatusCode}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"POST no data threw: {ex.Message}", false));
        }
    }

    async Task TestPostNoBodyNoResponse(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"POST /api/test/post", null));
            var result = await Contracts.ProcessAsync(new TestPostNoBodyNoResponseContract());

            var pass = result.IsSuccess;
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode}"));

            if (pass)
                _logs.Add(new LogEntry($"POST /api/test/post: SUCCESS", true));
            else
                _logs.Add(new LogEntry($"POST /api/test/post FAILED: status={result.StatusCode}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"POST /api/test/post threw: {ex.Message}", false));
        }
    }

    async Task TestPostSimpleBody(CategoryResult cat, string label)
    {
        try
        {
            var originalNumber = Random.Shared.NextInt64(100000);
            var seed = Random.Shared.Next(999);
            var expectedMsg = (originalNumber * seed).ToString();

            _logs.Add(new LogEntry($"POST /api/test/post-simple number={originalNumber} seed={seed}", null));
            var result = await Contracts.ProcessAsync(new TestPostSimpleBodyContract
            {
                Number = originalNumber,
                Seed = seed
            });

            if (!result.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"POST simple body FAILED: status={result.StatusCode}", false));
                return;
            }

            var match = result.Data.Message == expectedMsg;

            cat.Items.Add(new CategoryItem(label, match, $"client={expectedMsg} server={result.Data.Message}"));

            if (match)
                _logs.Add(new LogEntry($"POST simple body: VERIFIED - client and server both computed {result.Data.Message}", true));
            else
                _logs.Add(new LogEntry($"POST simple body: MISMATCH expected={expectedMsg} got={result.Data.Message}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"POST simple body threw: {ex.Message}", false));
        }
    }

    async Task TestPostComplexBody(CategoryResult cat, string label)
    {
        try
        {
            var mainNumber = Random.Shared.NextInt64(100000);
            var mainSeed = Random.Shared.Next(999);

            var dtos = new NumberDto[]
            {
                new NumberDto { Value = 31337, Seed = 11 },
                new NumberDto { Value = 98765, Seed = 42 },
                new NumberDto { Value = 12345L, Seed = 7 }
            };

            _logs.Add(new LogEntry($"POST /api/test/complex main={mainNumber} seed={mainSeed} extra[3]", null));
            var result = await Contracts.ProcessAsync(new TestPostComplexBodyContract
            {
                Number = mainNumber,
                Seed = mainSeed,
                Name = "TestName",
                Numbers = dtos
            });

            if (!result.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"POST complex body FAILED: status={result.StatusCode}", false));
                return;
            }

            var validationCode = ComplexResponse.Validate(mainSeed, result.Data);

            cat.Items.Add(new CategoryItem(label, 0 <= validationCode, $"validationCode={validationCode}"));

            if (0 <= validationCode)
                _logs.Add(new LogEntry($"POST complex body: validationCode={validationCode}", true));
            else
                _logs.Add(new LogEntry($"POST complex body: MISMATCH", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"POST complex body threw: {ex.Message}", false));
        }
    }

    async Task TestEncryptDecryptRoundTrip(CategoryResult cat)
    {
        try
        {
            var originalText = $"Secret message #{Random.Shared.Next(99999)}";
            var plaintext = System.Text.Encoding.UTF8.GetBytes(originalText);

            _logs.Add(new LogEntry($"POST /api/test/crypto/encrypt ({plaintext.Length} bytes)", null));

            var encryptResult = await Contracts.ProcessAsync(new TestEncryptContract
            {
                Plaintext = plaintext
            });

            if (!encryptResult.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem("encrypt", false, $"status={encryptResult.StatusCode}"));
                cat.Items.Add(new CategoryItem("decrypt", false, "skipped (encrypt failed)"));
                _logs.Add(new LogEntry($"Encrypt FAILED: status={encryptResult.StatusCode}", false));
                return;
            }

            var cryptoResult = encryptResult.Data;
            var cipherLen = cryptoResult.Ciphertext.Length;
            var ivLen = cryptoResult.Nonce.Length;
            var tagLen = cryptoResult.Tag.Length;
            cat.Items.Add(new CategoryItem("encrypt", true, $"cipher={cipherLen}B iv={ivLen}B tag={tagLen}B"));
            _logs.Add(new LogEntry($"Encrypt OK: ciphertext={cipherLen}B iv={ivLen}B tag={tagLen}B", true));

            _logs.Add(new LogEntry($"POST /api/test/crypto/decrypt ({cipherLen} bytes)", null));

            var decryptResult = await Contracts.ProcessAsync(new TestDecryptContract
            {
                Ciphertext = cryptoResult.Ciphertext,
                Nonce = cryptoResult.Nonce,
                Tag = cryptoResult.Tag
            });

            if (!decryptResult.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem("decrypt", false, $"status={decryptResult.StatusCode}"));
                _logs.Add(new LogEntry($"Decrypt FAILED: status={decryptResult.StatusCode}", false));
                return;
            }

            var decryptedText = System.Text.Encoding.UTF8.GetString(decryptResult.Data);
            var roundTripMatch = decryptedText == originalText;

            cat.Items.Add(new CategoryItem("decrypt", roundTripMatch, $"len={decryptResult.Data.Length}B"));

            if (roundTripMatch)
                _logs.Add(new LogEntry($"Decrypt: ROUND-TRIP VERIFIED plaintext matches!", true));
            else
                _logs.Add(new LogEntry($"Decrypt: MISMATCH got=\"{decryptedText}\" vs \"{originalText}\"", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem("encrypt", false, ex.Message));
            cat.Items.Add(new CategoryItem("decrypt", false, "skipped"));
            _logs.Add(new LogEntry($"Encrypt/Decrypt threw: {ex.Message}", false));
        }
    }

    async Task TestUploadSingleFile(CategoryResult cat, string label)
    {
        try
        {
            var fileContent = System.Text.Encoding.UTF8.GetBytes($"Test file #{Random.Shared.Next(99999)}");
            using var stream = new MemoryStream(fileContent);

            _logs.Add(new LogEntry($"POST /api/test/upload/single ({fileContent.Length} bytes)", null));

            var result = await Contracts.ProcessAsync(new TestUploadSingleFileContract
            {
                File = new ContractFile(stream, "test.txt", "text/plain", fileContent.Length)
            });

            if (!result.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"Upload single FAILED: status={result.StatusCode}", false));
                return;
            }

            var expectedSha = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileContent));
            var match = result.Data.Sha256 == expectedSha && result.Data.Length == fileContent.Length;

            cat.Items.Add(new CategoryItem(label, match, $"sha256={result.Data.Sha256[..12]}..."));

            if (match)
                _logs.Add(new LogEntry($"Upload single: VERIFIED SHA256 matches len={result.Data.Length}", true));
            else
                _logs.Add(new LogEntry($"Upload single: MISMATCH", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"Upload single threw: {ex.Message}", false));
        }
    }

    async Task TestUploadMultipleFiles(CategoryResult cat, string label)
    {
        try
        {
            var content1 = System.Text.Encoding.UTF8.GetBytes("File one!");
            using var ms1 = new MemoryStream(content1);
            var content2 = System.Text.Encoding.UTF8.GetBytes("File two is longer data.");
            using var ms2 = new MemoryStream(content2);

            _logs.Add(new LogEntry($"POST /api/test/upload/multiple (2 files)", null));

            var result = await Contracts.ProcessAsync(new TestUploadMultipleFilesContract
            {
                File1 = new ContractFile(ms1, "file1.txt", "text/plain", content1.Length),
                File2 = new ContractFile(ms2, "file2.txt", "text/plain", content2.Length)
            });

            if (!result.IsSuccessWithData || result.Data.Length != 2)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"Upload multiple FAILED: got items={result.Data?.Length}", false));
                return;
            }

            var sha1 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(content1));
            var sha2 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(content2));
            var match = result.Data[0].Sha256 == sha1 && result.Data[1].Sha256 == sha2;

            cat.Items.Add(new CategoryItem(label, match, $"file1={result.Data[0].Length}B file2={result.Data[1].Length}B"));

            if (match)
                _logs.Add(new LogEntry($"Upload multiple: VERIFIED both SHA256s match", true));
            else
                _logs.Add(new LogEntry($"Upload multiple: MISMATCH", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"Upload multiple threw: {ex.Message}", false));
        }
    }

    async Task TestUploadCombined(CategoryResult cat, string label)
    {
        try
        {
            var byteArrayData = new byte[64];
            Random.Shared.NextBytes(byteArrayData);

            var fileContent1 = System.Text.Encoding.UTF8.GetBytes("Combined test 1");
            using var ms1 = new MemoryStream(fileContent1);
            var fileContent2 = System.Text.Encoding.UTF8.GetBytes("Combined test 2 data here");
            using var ms2 = new MemoryStream(fileContent2);

            _logs.Add(new LogEntry($"POST /api/test/upload/combined (byte[] + 2 files)", null));

            var result = await Contracts.ProcessAsync(new TestUploadCombinedContract
            {
                Data = byteArrayData,
                File1 = new ContractFile(ms1, "f1.bin", "application/octet-stream", fileContent1.Length),
                File2 = new ContractFile(ms2, "f2.txt", "text/plain", fileContent2.Length)
            });

            if (!result.IsSuccessWithData || result.Data.Length != 3)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"Upload combined FAILED: got items={result.Data?.Length}", false));
                return;
            }

            var sha0 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(byteArrayData));
            var sha1 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileContent1));
            var sha2 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fileContent2));
            var match = result.Data[0].Sha256 == sha0 && result.Data[0].Length == byteArrayData.Length
                && result.Data[1].Sha256 == sha1 && result.Data[1].Length == fileContent1.Length
                && result.Data[2].Sha256 == sha2 && result.Data[2].Length == fileContent2.Length;

            var lenStr = "";
            foreach (var item in result.Data)
                lenStr += $"{item.Length}B ";

            cat.Items.Add(new CategoryItem(label, match, $"3 items: {lenStr.Trim()}"));

            if (match)
                _logs.Add(new LogEntry($"Upload combined: VERIFIED all 3 results correct", true));
            else
                _logs.Add(new LogEntry($"Upload combined: MISMATCH", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"Upload combined threw: {ex.Message}", false));
        }
    }

    async Task TestPutSimpleBody(CategoryResult cat, string label)
    {
        try
        {
            var originalNumber = Random.Shared.NextInt64(100000);
            var seed = Random.Shared.Next(999);

            _logs.Add(new LogEntry($"PUT /api/test/update number={originalNumber} seed={seed}", null));
            var result = await Contracts.ProcessAsync(new TestPutSimpleBodyContract
            {
                Number = originalNumber,
                Seed = seed
            });

            if (!result.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"PUT simple body FAILED: status={result.StatusCode}", false));
                return;
            }

            var validationCode = ComplexResponse.Validate((int)(originalNumber ^ seed), result.Data);

            cat.Items.Add(new CategoryItem(label, 0 <= validationCode, ""));

            if (0 <= validationCode)
                _logs.Add(new LogEntry($"PUT simple body: VERIFIED", true));
            else
                _logs.Add(new LogEntry($"PUT simple body: MISMATCH, validationCode={validationCode}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"PUT simple body threw: {ex.Message}", false));
        }
    }

    async Task TestPutRouteParam(CategoryResult cat, string label)
    {
        try
        {
            var originalNumber = Random.Shared.NextInt64(100000);
            var seed = Random.Shared.Next(999);

            _logs.Add(new LogEntry($"PUT /api/test/update/{seed} number={originalNumber}", null));
            var result = await Contracts.ProcessAsync(new TestPutRouteParamContract
            {
                Number = originalNumber,
                Seed = seed
            });

            if (!result.IsSuccessWithData)
            {
                cat.Items.Add(new CategoryItem(label, false, $"status={result.StatusCode}"));
                _logs.Add(new LogEntry($"PUT route param FAILED: status={result.StatusCode}", false));
                return;
            }

            var match = (originalNumber * seed).ToString() == result.Data.Message;

            cat.Items.Add(new CategoryItem(label, match, $"route={seed} msg={result.Data.Message}"));

            if (match)
                _logs.Add(new LogEntry($"PUT route param: VERIFIED msg={result.Data.Message}", true));
            else
                _logs.Add(new LogEntry($"PUT route param: MISMATCH expected={originalNumber * seed} got={result.Data.Message}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"PUT route param threw: {ex.Message}", false));
        }
    }

    async Task TestDeleteNoBodyNoResponse(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry($"DELETE /api/test/remove", null));
            var result = await Contracts.ProcessAsync(new TestDeleteNoBodyNoResponseContract());

            var pass = result.IsSuccess;
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode}"));

            if (pass)
                _logs.Add(new LogEntry($"DELETE /api/test/remove: SUCCESS", true));
            else
                _logs.Add(new LogEntry($"DELETE /api/test/remove FAILED: status={result.StatusCode}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"DELETE threw: {ex.Message}", false));
        }
    }

    async Task TestCompressEcho(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry("POST /api/test/compress/echo (lz4)", null));
            var result = await Contracts.ProcessAsync(new TestCompressEchoContract { Message = "lz4 compression" });

            var pass = result.IsSuccessWithData && result.Data.Message == "Compressed echo: lz4 compression";
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode} {result.ErrorMessage}"));

            if (pass)
                _logs.Add(new LogEntry("compressed json round trip ok", true));
            else
                _logs.Add(new LogEntry($"compressed json FAILED: status={result.StatusCode} {result.ErrorMessage}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"compressed json threw: {ex.Message}", false));
        }
    }

    async Task TestCompressBinary(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry("POST /api/test/compress/binary (lz4 multipart)", null));
            var payload = Enumerable.Range(0, 4096).Select(i => (byte)(i / 16)).ToArray();
            var result = await Contracts.ProcessAsync(new TestCompressBinaryContract { Content = payload, FileName = "page.bin" });

            var pass = result.IsSuccessWithData
                && result.Data.Length == "echo:".Length + payload.Length
                && result.Data.Skip("echo:".Length).SequenceEqual(payload);
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode} {result.ErrorMessage} len={result.Data?.Length}"));

            if (pass)
                _logs.Add(new LogEntry("compressed binary round trip ok", true));
            else
                _logs.Add(new LogEntry($"compressed binary FAILED: status={result.StatusCode} {result.ErrorMessage}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"compressed binary threw: {ex.Message}", false));
        }
    }

    async Task TestCompressMessagePack(CategoryResult cat, string label)
    {
        try
        {
            _logs.Add(new LogEntry("POST /api/msgpack/compress (lz4 msgpack)", null));
            var result = await Contracts.ProcessAsync(new MessagePackCompressContract { Seed = 33, Days = 4, City = "Lz4 City" });

            var pass = result.IsSuccessWithData && result.Data.Summary == "Lz4 City";
            cat.Items.Add(new CategoryItem(label, pass, $"status={result.StatusCode} {result.ErrorMessage}"));

            if (pass)
                _logs.Add(new LogEntry("compressed msgpack round trip ok", true));
            else
                _logs.Add(new LogEntry($"compressed msgpack FAILED: status={result.StatusCode} {result.ErrorMessage}", false));
        }
        catch (Exception ex)
        {
            cat.Items.Add(new CategoryItem(label, false, ex.Message));
            _logs.Add(new LogEntry($"compressed msgpack threw: {ex.Message}", false));
        }
    }

    // ---------------------------------------------------------------------
    // Shared UI state types
    // ---------------------------------------------------------------------

    sealed class CategoryResult
    {
        public string Name { get; set; }
        public List<CategoryItem> Items { get; set; } = new();
        public int Total => Items.Count;
        public int Passed => Items.Count(i => i.Pass);
        public bool AllPass => Passed == Total;

        public CategoryResult(string name) => Name = name;
    }

    sealed class CategoryItem
    {
        public string Label { get; set; }
        public bool Pass { get; set; }
        public string Detail { get; set; }

        public CategoryItem(string label, bool pass, string detail)
        {
            Label = label;
            Pass = pass;
            Detail = detail;
        }
    }

    sealed class LogEntry
    {
        public DateTime Timestamp => created;
        public string Message { get; set; }
        public bool? Success { get; set; }
        public string Type { get; set; }

        readonly DateTime created = DateTime.Now;

        public LogEntry(string message, bool? success)
        {
            Message = message;
            Success = success;
            Type = success == true ? "bg-light" : success == false ? "bg-danger bg-opacity-10" : "";
        }
    }

    private sealed record MaterializedResponse(int Status, string? ContentType, bool Lz4, byte[] Body)
    {
        public string BodyText => Encoding.UTF8.GetString(Body);
    }

    private static async Task<MaterializedResponse> MaterializeResponseAsync(HttpResponseMessage response)
    {
        byte[] body = [];
        string? contentType = null;

        if (response.Content is not null)
        {
            body = await response.Content.ReadAsByteArrayAsync();
            contentType = response.Content.Headers.ContentType?.ToString();
        }

        return new MaterializedResponse((int)response.StatusCode, contentType, ContractHeaders.IsLz4Response(response), body);
    }

    private static (int status, string? error, JsonElement? data) ParseJsonEnvelope(byte[] body)
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var status = root.TryGetProperty("statusCode", out var st) ? st.GetInt32() : -1;
        string? error = root.TryGetProperty("errorMessage", out var er) ? er.GetString() : null;
        JsonElement? data = root.TryGetProperty("data", out var d) ? d.Clone() : null;
        return (status, error, data);
    }

    private static ByteArrayContent JsonContent(string json)
    {
        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        content.Headers.ContentType = new MediaTypeHeaderValue(ContractHeaders.JsonMediaType);
        return content;
    }

    private static ByteArrayContent MsgpackContent(byte[] payload)
    {
        var content = new ByteArrayContent(payload);
        content.Headers.ContentType = new MediaTypeHeaderValue(ContractHeaders.MessagePackMediaType);
        return content;
    }

    private static async Task<HttpContent> Lz4Wrap(byte[] plain, string mediaType)
    {
        await using var ms = new MemoryStream();
        {
            await using var encoder = LZ4Stream.Encode(ms);
            await encoder.WriteAsync(plain);
        }
        var content = new ByteArrayContent(ms.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        content.Headers.TryAddWithoutValidation(ContractHeaders.XContentEncodingHeader, ContractHeaders.XContentEncodingHeaderLz4);
        return content;
    }

    private static async Task<byte[]> Lz4Unwrap(byte[] compressed)
    {
        await using var ms = new MemoryStream(compressed, false);
        await using var decoder = LZ4Stream.Decode(ms);
        await using var msOut = new MemoryStream();
        await decoder.CopyToAsync(msOut);
        return msOut.ToArray();
    }

    private static string Sha256Hex(byte[] data)
        => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data));

    private async Task RunWireFormatsAsync()
    {
        var cat = new CategoryResult("Wire Formats (raw HTTP)");
        using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };

        await RunCaseAsync(cat, "GET envelope: exact json shape + content-type", async () =>
        {
            using var response = await client.GetAsync("/api/test/simple");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/json; charset=utf-8"
                && !materialized.Lz4
                && status == 200
                && error is null
                && data is not null
                && data.Value.TryGetProperty("Message", out var msg)
                && int.TryParse(msg.GetString(), out _);

            Pass(cat, "GET envelope: exact json shape + content-type", pass,
                $"ct={materialized.ContentType} status={status} body={materialized.BodyText}");
        });

        await RunCaseAsync(cat, "GET no-data: 204 with empty body", async () =>
        {
            using var response = await client.GetAsync("/api/test");
            var materialized = await MaterializeResponseAsync(response);

            var pass = materialized.Status == 204 && materialized.Body.Length == 0;

            Pass(cat, "GET no-data: 204 with empty body", pass, $"status={materialized.Status} len={materialized.Body.Length}");
        });

        await RunCaseAsync(cat, "GET health: 200 json envelope", async () =>
        {
            using var response = await client.GetAsync("/api/health");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, _) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && error is null
                && materialized.ContentType == "application/json; charset=utf-8";

            Pass(cat, "GET health: 200 json envelope", pass, $"status={status} ct={materialized.ContentType}");
        });

        await RunCaseAsync(cat, "GET stream: application/octet-stream + exact bytes", async () =>
        {
            using var response = await client.GetAsync("/api/test/stream");
            var materialized = await MaterializeResponseAsync(response);

            var expected = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            var pass = materialized.Status == 200
                && materialized.ContentType == "application/octet-stream"
                && !materialized.Lz4
                && materialized.Body.SequenceEqual(expected);

            Pass(cat, "GET stream: application/octet-stream + exact bytes", pass,
                $"ct={materialized.ContentType} len={materialized.Body.Length} lz4={materialized.Lz4}");
        });

        await RunCaseAsync(cat, "GET compressed stream: lz4 header + decodable frame", async () =>
        {
            using var response = await client.GetAsync("/api/test/compressed-stream");
            var materialized = await MaterializeResponseAsync(response);
            var plain = await Lz4Unwrap(materialized.Body);

            var expected = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            var pass = materialized.Status == 200
                && materialized.ContentType == "application/octet-stream"
                && materialized.Lz4
                && plain.SequenceEqual(expected);

            Pass(cat, "GET compressed stream: lz4 header + decodable frame", pass,
                $"ct={materialized.ContentType} wireLen={materialized.Body.Length} plainLen={plain.Length} lz4={materialized.Lz4}");
        });

        await RunCaseAsync(cat, "GET compressed bytes: lz4 octet-stream round trip", async () =>
        {
            using var response = await client.GetAsync("/api/test/compressed-bytes");
            var materialized = await MaterializeResponseAsync(response);
            var plain = await Lz4Unwrap(materialized.Body);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/octet-stream"
                && materialized.Lz4
                && plain.Length == 512
                && Enumerable.Range(0, 512).All(i => plain[i] == (byte)(i % 256));

            Pass(cat, "GET compressed bytes: lz4 octet-stream round trip", pass,
                $"wireLen={materialized.Body.Length} plainLen={plain.Length}");
        });

        await RunCaseAsync(cat, "GET compressed envelope: lz4 json decodes to envelope", async () =>
        {
            using var response = await client.GetAsync("/api/test/compressed-envelope?Echo=wire-lz4");
            var materialized = await MaterializeResponseAsync(response);
            var plain = await Lz4Unwrap(materialized.Body);
            var (status, error, data) = ParseJsonEnvelope(plain);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/json; charset=utf-8"
                && materialized.Lz4
                && status == 200
                && error is null
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Compressed envelope echo: wire-lz4";

            Pass(cat, "GET compressed envelope: lz4 json decodes to envelope", pass,
                $"lz4={materialized.Lz4} status={status} plain={Encoding.UTF8.GetString(plain)}");
        });

        await RunCaseAsync(cat, "GET msgpack envelope: application/x-msgpack body", async () =>
        {
            using var response = await client.GetAsync("/api/test/msgpack-envelope?Echo=wire-mp");
            var materialized = await MaterializeResponseAsync(response);

            await using var ms = new MemoryStream(materialized.Body);
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<SimpleTestResponse>>(ms);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/x-msgpack"
                && envelope is { StatusCode: 200, Data.Message: "Msgpack envelope echo: wire-mp" };

            Pass(cat, "GET msgpack envelope: application/x-msgpack body", pass,
                $"ct={materialized.ContentType} msg={envelope?.Data?.Message}");
        });

        await RunCaseAsync(cat, "POST lz4 json -> lz4 json envelope round trip", async () =>
        {
            var json = """{"Message":"wire-compressed"}""";
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/compress/echo")
            {
                Content = await Lz4Wrap(Encoding.UTF8.GetBytes(json), ContractHeaders.JsonMediaType)
            };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);

            var (status, error, data) = ParseJsonEnvelope(await Lz4Unwrap(materialized.Body));

            var pass = materialized.Status == 200
                && materialized.Lz4
                && materialized.ContentType == "application/json; charset=utf-8"
                && status == 200
                && error is null
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Compressed echo: wire-compressed";

            Pass(cat, "POST lz4 json -> lz4 json envelope round trip", pass,
                $"lz4={materialized.Lz4} status={status}");
        });

        await RunCaseAsync(cat, "POST msgpack -> msgpack envelope + payload echo", async () =>
        {
            var payload = new byte[300];
            Random.Shared.NextBytes(payload);

            var contract = new MessagePackWeatherContract { Seed = 5, Days = 3, Payload = payload };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/msgpack/weather")
            {
                Content = MsgpackContent(BlazorContractsMessagePack.Serialize(contract))
            };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);

            await using var ms = new MemoryStream(materialized.Body);
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<MessagePackWeatherForecast>>(ms);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/x-msgpack"
                && !materialized.Lz4
                && envelope is { StatusCode: 200, Data: not null }
                && envelope.Data.Payload.SequenceEqual(payload)
                && envelope.Data.Days is { Length: 3 };

            Pass(cat, "POST msgpack -> msgpack envelope + payload echo", pass,
                $"ct={materialized.ContentType} payload={envelope?.Data?.Payload.Length ?? 0}B days={envelope?.Data?.Days?.Length ?? 0}");
        });

        await RunCaseAsync(cat, "POST lz4 msgpack -> lz4 msgpack envelope", async () =>
        {
            var contract = new MessagePackCompressContract { Seed = 9, Days = 2, City = "WireLz4" };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/msgpack/compress")
            {
                Content = await Lz4Wrap(BlazorContractsMessagePack.Serialize(contract), ContractHeaders.MessagePackMediaType)
            };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);

            await using var decoder = LZ4Stream.Decode(new MemoryStream(materialized.Body));
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<MessagePackWeatherForecast>>(decoder);

            var pass = materialized.Status == 200
                && materialized.Lz4
                && materialized.ContentType == "application/x-msgpack"
                && envelope is { StatusCode: 200, Data.Summary: "WireLz4" };

            Pass(cat, "POST lz4 msgpack -> lz4 msgpack envelope", pass,
                $"lz4={materialized.Lz4} ct={materialized.ContentType} summary={envelope?.Data?.Summary}");
        });

        await RunCaseAsync(cat, "POST msgpack byte[] echo -> msgpack envelope bin", async () =>
        {
            var payload = new byte[999];
            Random.Shared.NextBytes(payload);

            var contract = new TestMsgpackBytesEchoContract { Payload = payload };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/msgpack-bytes-echo")
            {
                Content = MsgpackContent(BlazorContractsMessagePack.Serialize(contract))
            };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);

            await using var ms = new MemoryStream(materialized.Body);
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<byte[]>>(ms);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/x-msgpack"
                && envelope is { StatusCode: 200 }
                && envelope.Data is not null
                && Sha256Hex(envelope.Data) == Sha256Hex(payload);

            Pass(cat, "POST msgpack byte[] echo -> msgpack envelope bin", pass,
                $"ct={materialized.ContentType} len={envelope?.Data?.Length}");
        });

        await RunCaseAsync(cat, "POST multipart by hand: json part + bin part + 2 file parts", async () =>
        {
            var dataBytes = new byte[128];
            Random.Shared.NextBytes(dataBytes);
            var file1 = Encoding.UTF8.GetBytes("wire file one");
            var file2 = Encoding.UTF8.GetBytes("wire file two with more content");

            using var multipart = new MultipartFormDataContent("wire-boundary-" + Guid.NewGuid().ToString("N"));
            multipart.Add(JsonContent("{}"), "envTestUploadCombinedContract");
            multipart.Add(ContractBody.CreateBinary(dataBytes, ContractHeaders.OctetStreamMediaType), "propData", "Data.bin");
            multipart.Add(ContractBody.CreateFile(new ContractFile(new MemoryStream(file1), "w1.txt", "text/plain", file1.Length)), "propFile1", "w1.txt");
            multipart.Add(ContractBody.CreateFile(new ContractFile(new MemoryStream(file2), "w2.txt", "text/plain", file2.Length)), "propFile2", "w2.txt");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/upload/combined") { Content = multipart };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && error is null
                && data is not null
                && data.Value.GetArrayLength() == 3
                && data.Value[0].GetProperty("Sha256").GetString() == Sha256Hex(dataBytes)
                && data.Value[1].GetProperty("Sha256").GetString() == Sha256Hex(file1)
                && data.Value[2].GetProperty("Sha256").GetString() == Sha256Hex(file2);

            Pass(cat, "POST multipart by hand: json part + bin part + 2 file parts", pass,
                $"status={status} items={(data is null ? -1 : data.Value.GetArrayLength())} err={error}");
        });

        await RunCaseAsync(cat, "POST compressed multipart: every part lz4-framed", async () =>
        {
            var payload = Enumerable.Range(0, 2048).Select(i => (byte)(i % 251)).ToArray();

            using var multipart = new MultipartFormDataContent("wire-lz4-boundary-" + Guid.NewGuid().ToString("N"));
            multipart.Add(await Lz4Wrap(Encoding.UTF8.GetBytes("""{"FileName":"wire-lz4.bin"}"""), ContractHeaders.JsonMediaType), "envTestCompressBinaryContract");
            multipart.Add(await Lz4Wrap(payload, ContractHeaders.OctetStreamMediaType), "propContent", "Content.bin");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/compress/binary") { Content = multipart };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);
            var plain = await Lz4Unwrap(materialized.Body);

            var expected = "echo:"u8.ToArray().Concat(payload).ToArray();
            var pass = materialized.Status == 200
                && materialized.ContentType == "application/octet-stream"
                && materialized.Lz4
                && plain.SequenceEqual(expected);

            Pass(cat, "POST compressed multipart: every part lz4-framed", pass,
                $"status={materialized.Status} wireLen={materialized.Body.Length} plainLen={plain.Length} lz4={materialized.Lz4}");
        });

        await RunCaseAsync(cat, "POST multipart msgpack contract part + file part", async () =>
        {
            var fileBytes = new byte[640];
            Random.Shared.NextBytes(fileBytes);

            var contract = new MessagePackStreamUploadContract
            {
                FileName = "wire-mp.bin",
                MediaType = "application/octet-stream"
            };

            using var multipart = new MultipartFormDataContent("wire-mp-boundary-" + Guid.NewGuid().ToString("N"));
            multipart.Add(MsgpackContent(BlazorContractsMessagePack.Serialize(contract)), "envMessagePackStreamUploadContract", "MessagePackStreamUploadContract.msgpack");
            multipart.Add(ContractBody.CreateBinary(fileBytes, ContractHeaders.OctetStreamMediaType), "propContent", "wire-mp.bin");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/msgpack/stream-upload") { Content = multipart };
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);

            await using var ms = new MemoryStream(materialized.Body);
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<MessagePackBinaryResponse>>(ms);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/x-msgpack"
                && envelope is { StatusCode: 200, Data: not null }
                && envelope.Data.Content.SequenceEqual(fileBytes)
                && envelope.Data.FileName == "wire-mp.bin"
                && envelope.Data.Sha256 == Sha256Hex(fileBytes);

            Pass(cat, "POST multipart msgpack contract part + file part", pass,
                $"ct={materialized.ContentType} id={envelope?.Data?.Id} shaOK={envelope?.Data?.Sha256 == Sha256Hex(fileBytes)}");
        });

        await RunCaseAsync(cat, "POST multipart file upload then raw bytes/stream download", async () =>
        {
            var fileBytes = new byte[410];
            Random.Shared.NextBytes(fileBytes);

            using var multipart = new MultipartFormDataContent("wire-up-boundary-" + Guid.NewGuid().ToString("N"));
            multipart.Add(JsonContent("""{"FileName":"wire-raw.bin","MediaType":"application/octet-stream"}"""), "envFileUploadBytesContract");
            multipart.Add(ContractBody.CreateBinary(fileBytes, ContractHeaders.OctetStreamMediaType), "propContent", "Content.bin");

            using var upload = new HttpRequestMessage(HttpMethod.Post, "/api/files/bytes") { Content = multipart };
            var uploadResponse = await client.SendAsync(upload);
            var uploadWire = await MaterializeResponseAsync(uploadResponse);
            var (upStatus, _, upData) = ParseJsonEnvelope(uploadWire.Body);

            var uploadOk = uploadWire.Status == 200 && upStatus == 200 && upData is { };
            if (!uploadOk)
            {
                Pass(cat, "POST multipart file upload then raw bytes/stream download", false,
                    $"upload status={uploadWire.Status} body={uploadWire.BodyText}");
                return;
            }

            var id = upData!.Value.GetProperty("Id").GetInt32();

            var bytesResponse = await client.GetAsync($"/api/files/{id}/bytes");
            var bytesWire = await MaterializeResponseAsync(bytesResponse);

            var streamResponse = await client.GetAsync($"/api/files/{id}/stream");
            var streamWire = await MaterializeResponseAsync(streamResponse);

            var pass = uploadWire.ContentType == "application/json; charset=utf-8"
                && bytesWire.Status == 200
                && bytesWire.ContentType == "application/octet-stream"
                && bytesWire.Body.SequenceEqual(fileBytes)
                && streamWire.Status == 200
                && streamWire.ContentType == "application/octet-stream"
                && streamWire.Body.SequenceEqual(fileBytes);

            Pass(cat, "POST multipart file upload then raw bytes/stream download", pass,
                $"id={id} bytes={bytesWire.Body.Length}B stream={streamWire.Body.Length}B");
        });

        await RunCaseAsync(cat, "GET missing file: 404 json error envelope", async () =>
        {
            using var response = await client.GetAsync("/api/files/999999/bytes");
            var materialized = await MaterializeResponseAsync(response);
            var (status, error, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 404
                && status == 404
                && error is not null && error.Contains("not found")
                && data is null
                && materialized.ContentType == "application/json; charset=utf-8";

            Pass(cat, "GET missing file: 404 json error envelope", pass,
                $"status={status} err={error} ct={materialized.ContentType}");
        });

        await RunCaseAsync(cat, "DELETE with query params on the wire", async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/test/delete-query?query_filter=wire&page_number=7");
            using var response = await client.SendAsync(request);
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == "Filter=wire;Page=7";

            Pass(cat, "DELETE with query params on the wire", pass, $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())}");
        });

        await RunCaseAsync(cat, "string route param escaping on the wire", async () =>
        {
            var name = "a b&c=d?e 100%! 日本語";
            using var response = await client.GetAsync($"/api/test/route-string/{Uri.EscapeDataString(name)}");
            var materialized = await MaterializeResponseAsync(response);
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetProperty("Message").GetString() == name;

            Pass(cat, "string route param escaping on the wire", pass,
                $"msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())}");
        });

        await RunCaseAsync(cat, "route param type mismatch -> 400", async () =>
        {
            using var response = await client.GetAsync("/api/test/query/abc?Filter=x");
            var materialized = await MaterializeResponseAsync(response);

            var pass = materialized.Status == 400;

            Pass(cat, "route param type mismatch -> 400", pass, $"status={materialized.Status}");
        });

        _categories.Add(cat);
        Recount();
    }

    private async Task RunMisalignedServerAsync()
    {
        var cat = new CategoryResult("Misaligned Formats (server)");
        using var client = new HttpClient() { BaseAddress = new Uri(NavMgr.BaseUri) };

        await RunCaseAsync(cat, "json contract + msgpack body -> 200 (content-type driven)", async () =>
        {
            var contract = new TestPostSimpleBodyContract { Number = 6, Seed = 7 };
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/post-simple")
            {
                Content = MsgpackContent(BlazorContractsMessagePack.Serialize(contract))
            };
            var materialized = await MaterializeResponseAsync(await client.SendAsync(request));
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200 && status == 200
                && data is not null && data.Value.GetProperty("Message").GetString() == "42";

            Pass(cat, "json contract + msgpack body -> 200 (content-type driven)", pass,
                $"status={status} msg={(data is null ? "n/a" : data.Value.GetProperty("Message").GetString())}");
        });

        await RunCaseAsync(cat, "lz4 json + header -> 200 (positive control)", async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/post-simple")
            {
                Content = await Lz4Wrap(Encoding.UTF8.GetBytes("""{"Number":12,"Seed":3}"""), ContractHeaders.JsonMediaType)
            };
            var materialized = await MaterializeResponseAsync(await client.SendAsync(request));
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200 && status == 200
                && data is not null && data.Value.GetProperty("Message").GetString() == "36";

            Pass(cat, "lz4 json + header -> 200 (positive control)", pass, $"status={status}");
        });

        await RunCaseAsync(cat, "msgpack contract + json body -> 200 (content-type driven)", async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/msgpack/weather")
            {
                Content = JsonContent("""{"Seed":4,"Days":2}""")
            };
            var materialized = await MaterializeResponseAsync(await client.SendAsync(request));

            await using var ms = new MemoryStream(materialized.Body);
            var envelope = await BlazorContractsMessagePack.DeserializeAsync<ContractResult<MessagePackWeatherForecast>>(ms);

            var pass = materialized.Status == 200
                && materialized.ContentType == "application/x-msgpack"
                && envelope is { StatusCode: 200, Data.Days.Length: 2 };

            Pass(cat, "msgpack contract + json body -> 200 (content-type driven)", pass,
                $"status={materialized.Status} ct={materialized.ContentType} days={envelope?.Data?.Days?.Length ?? 0}");
        });

        await RunCaseAsync(cat, "json body with byte[] field -> 400 (json carries no binary)", async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/msgpack-bytes-echo")
            {
                Content = JsonContent("""{"Payload":"AQIDBA=="}""")
            };
            var materialized = await MaterializeResponseAsync(await client.SendAsync(request));

            var pass = materialized.Status == 400
                && materialized.ContentType == "application/x-msgpack";

            Pass(cat, "json body with byte[] field -> 400 (json carries no binary)", pass,
                $"status={materialized.Status} ct={materialized.ContentType}");
        });

        await RunCaseAsync(cat, "multipart msgpack contract part on json contract -> 200", async () =>
        {
            var dataBytes = new byte[64];
            Random.Shared.NextBytes(dataBytes);
            var file1 = Encoding.UTF8.GetBytes("part-level msgpack");

            var contract = new TestUploadCombinedContract { Data = dataBytes };

            using var multipart = new MultipartFormDataContent("mis-3-" + Guid.NewGuid().ToString("N"));
            multipart.Add(MsgpackContent(BlazorContractsMessagePack.Serialize(contract)), "envTestUploadCombinedContract");
            multipart.Add(ContractBody.CreateBinary(dataBytes, ContractHeaders.OctetStreamMediaType), "propData", "Data.bin");
            multipart.Add(ContractBody.CreateFile(new ContractFile(new MemoryStream(file1), "m1.txt", "text/plain", file1.Length)), "propFile1", "m1.txt");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/upload/combined") { Content = multipart };
            var materialized = await MaterializeResponseAsync(await client.SendAsync(request));
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetArrayLength() == 2
                && data.Value[0].GetProperty("Sha256").GetString() == Sha256Hex(dataBytes)
                && data.Value[1].GetProperty("Sha256").GetString() == Sha256Hex(file1);

            Pass(cat, "multipart msgpack contract part on json contract -> 200", pass,
                $"status={status} items={(data is null ? -1 : data.Value.GetArrayLength())}");
        });

        await RunCaseAsync(cat, "multipart contract part without content-type -> 200", async () =>
        {
            var dataBytes = new byte[32];
            Random.Shared.NextBytes(dataBytes);

            var barePart = new ByteArrayContent(Encoding.UTF8.GetBytes("{}"));

            using var multipart = new MultipartFormDataContent("mis-4-" + Guid.NewGuid().ToString("N"));
            multipart.Add(barePart, "envTestUploadCombinedContract");
            multipart.Add(ContractBody.CreateBinary(dataBytes, ContractHeaders.OctetStreamMediaType), "propData", "Data.bin");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/test/upload/combined") { Content = multipart };
            var materialized = await MaterializeResponseAsync(await client.SendAsync(request));
            var (status, _, data) = ParseJsonEnvelope(materialized.Body);

            var pass = materialized.Status == 200
                && status == 200
                && data is not null
                && data.Value.GetArrayLength() == 1
                && data.Value[0].GetProperty("Sha256").GetString() == Sha256Hex(dataBytes);

            Pass(cat, "multipart contract part without content-type -> 200", pass,
                $"status={status} items={(data is null ? -1 : data.Value.GetArrayLength())}");
        });

        _categories.Add(cat);
        Recount();
    }
}

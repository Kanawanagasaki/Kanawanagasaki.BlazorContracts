1. GET/DELETE query parameters: for non-nullable, non-reference properties, check against default and omit the parameter when equal to default
2. GET/DELETE query parameters, if property have json property name, use that for query parameter name
3. Replace ToString with ISpanFormattable, IFormattable, IUtf8SpanFormattable, ISpanParsable, IParsable, IUtf8SpanParsable for properties that are being serialized to strings 
4. Use json property name attribute for the query keys if json property name attribute is assigned to the property
5. Malformed POST/PUT body without binary properties must return 400 contract result instead of letting asp.net core return default model binding 400 error
6. Replace System.Web.HttpUtility.UrlEncode with Uri.EscapeDataString
7. Replace all `var query = System.Web.HttpUtility.ParseQueryString(string.Empty);` query building code with `QueryHelpers.AddQueryString`
8. Handle route parameters assignation for POST/PUT using [UnsafeAccessor] instead of trying to assign property directly (for example for [Contract("/resource/{Id}", EVerbs.Put)])
9. Endpoints - add [FromServices] for injected services
10. ContractsService (Generated server service) - remove injected services bloat from constructor and fields, inject only IServiceProvider, get only required (or nullable) services for a contract in ProcessAsync method. If service is required property or in constructor then it is required service - GetRequiredService<>, otherwise GetService<>
11. Remove stupid double underscore from the variable names in generated code if there is no way of user/generated code conflict
12. Refactor code so duplicate generated code (for example checking response media type in every ProcessAsync method to determine if it is json or messagepack) will be moved into generalized methods instead
13. Make it so client and server are wire-format agnostic - always check content type headers and act on it, if contract is decorated for message pack but client sends content type - json then server should try to parse body as json, same when server returns json content type for contract response decorated with message pack. The reason being is that server owner can change serialization but clients can be slow to update and continue to send object serialized old way. That also means that byte arrays, contract files and streams must always be handled for json, message pack and multipart paths
14. Add support for compressing contract requests and responses using K4os.Compression.LZ4:
	Create new attribute - [ContractCompression] that will accept configuration for LZ4Level speed, if not set then by default use LZ4Level.L00_FAST
	If contract is decorated with [ContractCompression] then both request and response must be compressed (unline message pack where message pack can be set for request but not a response or vice versa)
	Json request and response should be encoding utf8 to byte arrayed and then compressed
	Message pack already byte arrayed, just compress it
	byte array should be just compressed
	Streams should be LZ4EncoderStream-ed and LZ4DecoderStream-ed
	Client must set "X-Content-Encoding: lz4" header for their requests if contract is decorated for compression
	Server must act on "X-Content-Encoding" header - if header set and set to "lz4" then try to decompress, otherwise parse as uncompressed
	If contract is decorated for compression then server must set "X-Content-Encoding: lz4" header
	When client reads response from server it should act on "X-Content-Encoding" header - if header is set and is "lz4" then decompress (then act on content type), otherwise parse as normal response based on content type. (just like json/message pack, which should always check content type)
	multipart requests and responses should compress only their parts - json part, message pack part, binary part is compressed etc, never the whole multipart
	server and client must act on each part's "X-Content-Encoding" header
15. Add tests to test page for missing features
16. Fix `CS0128, build failure` when handler is decorated with multiple [Authorize] policy attributes
17. Fix `CS0266` when contract response type is `class Resource : IDisposable { public byte[] Content { get; set; } = []; public void Dispose() { } }`
18. Check (and fix if broken) properties with only getters, with getters and non public setters, for GET/POST/PUT/DELETE. Potential problem is assignation of parameters from query and route, json and message pack serialization and deserialization will handle such cases by themselves
19. Check by setting { get; init; } on all properties for all contracts in demo project and trying to build. Project should work with all public properties and should work with { get; init; } - public getter and public init without any problem
20. Fix ExcludeFromDescriptionAttribute, DisableOutputCacheAttribute, Microsoft.AspNetCore.Antiforgery.IgnoreAntiforgeryTokenAttribute
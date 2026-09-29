namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

using MessagePack;

/// <summary>
/// A response model marked with [MessagePackObject] so the source generator emits a
/// MessagePack response body (Content-Type: application/x-msgpack) instead of JSON.
/// </summary>
[MessagePackObject]
public class MessagePackWeatherForecast
{
    [Key(0)]
    public DateTime Date { get; set; }

    [Key(1)]
    public int TemperatureC { get; set; }

    [Key(2)]
    public string Summary { get; set; } = string.Empty;

    [Key(3)]
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    [Key(4)]
    public MessagePackWeatherDay[]? Days { get; set; }

    [IgnoreMember]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

[MessagePackObject]
public class MessagePackWeatherDay
{
    [Key(0)]
    public DateTime Date { get; set; }

    [Key(1)]
    public int TemperatureC { get; set; }

    [Key(2)]
    public string Summary { get; set; } = string.Empty;
}

namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

public class BaseLoggerHandler
{
    public required ILogger<BaseLoggerHandler> Logger { get; init; }
}

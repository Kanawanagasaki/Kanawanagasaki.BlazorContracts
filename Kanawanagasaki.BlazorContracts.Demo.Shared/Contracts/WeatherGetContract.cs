namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/weather", EVerbs.Get)]
public class WeatherGetContract : IContract<WeatherForecast[]>
{
    public string? City { get; set; }
    public bool WithSummary { get; set; }
}

namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;
using Kanawanagasaki.BlazorContracts.Demo.Shared.Models;

[Contract("/api/weather/{Id}", EVerbs.Get)]
public class WeatherGetByIdContract : IContract<WeatherForecast>
{
    public int Id { get; set; }
}

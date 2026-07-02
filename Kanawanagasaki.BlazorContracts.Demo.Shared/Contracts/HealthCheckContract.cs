namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/health", EVerbs.Get)]
public class HealthCheckContract : IContract { }

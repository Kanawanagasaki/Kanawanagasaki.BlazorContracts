namespace Kanawanagasaki.BlazorContracts.Demo.Shared.Contracts;

using Kanawanagasaki.BlazorContracts;

[Contract("/api/test/file-with-error", EVerbs.Get)]
public class TestGetFileWithErrorContract : IContract<ContractFile>
{ }

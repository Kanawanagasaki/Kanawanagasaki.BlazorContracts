namespace Kanawanagasaki.BlazorContracts;

public class ContractAttribute(string endpoint, EVerbs verb) : Attribute
{
    public string Endpoint { get; } = endpoint;
    public EVerbs Verb { get; } = verb;
}

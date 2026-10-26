namespace Kanawanagasaki.BlazorContracts.Demo.Handlers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class DemoAuditAttribute : Attribute
{
    public string AuditSource { get; }
    public int Level { get; set; }

    public DemoAuditAttribute(string auditSource)
    {
        AuditSource = auditSource;
    }
}

namespace Kanawanagasaki.BlazorContracts.SourceGenerator.Shared;

using Microsoft.CodeAnalysis;

public class PropertyMetadata
{
    public IPropertySymbol PropSymb { get; }
    public string Name => PropSymb.Name;
    public string FullyQualifiedName => PropSymb.Type.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT_GENERICS);

    public bool IsByteArray { get; }
    public bool IsContractFile { get; }
    public bool IsReferenceType { get; }
    public bool IsNullable { get; }

    public PropertyMetadata(IPropertySymbol propSymb)
    {
        PropSymb = propSymb;

        IsByteArray = propSymb.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT) == typeof(byte).FullName;
        IsContractFile = propSymb.Type.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT) == "Kanawanagasaki.BlazorContracts.ContractFile";
        IsReferenceType = propSymb.Type.IsReferenceType;
        IsNullable = propSymb.Type.NullableAnnotation is NullableAnnotation.Annotated;
    }
}

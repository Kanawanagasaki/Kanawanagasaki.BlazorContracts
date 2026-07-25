namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Microsoft.CodeAnalysis;

public class PropertyMetadata
{
    public string Name { get; }
    public string FullyQualifiedName { get; }

    public bool IsByteArray { get; }
    public bool IsContractFile { get; }
    public bool IsReferenceType { get; }
    public bool IsNullable { get; }

    public PropertyMetadata(IPropertySymbol propSymb)
    {
        Name = propSymb.Name;
        FullyQualifiedName = propSymb.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);

        IsByteArray = propSymb.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(byte).FullName;
        IsContractFile = propSymb.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == "Kanawanagasaki.BlazorContracts.ContractFile";
        IsReferenceType = propSymb.Type.IsReferenceType;
        IsNullable = propSymb.Type.NullableAnnotation is NullableAnnotation.Annotated;
    }
}

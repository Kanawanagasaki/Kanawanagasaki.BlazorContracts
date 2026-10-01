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

    public bool HasPublicSetter { get; }
    public bool IsInitOnly { get; }

    public string? JsonPropertyName { get; }

    public bool IsFormattable { get; }

    private static readonly HashSet<string> FormattableInterfaceNames =
    [
        "IFormattable", "ISpanFormattable", "IUtf8SpanFormattable"
    ];

    public PropertyMetadata(IPropertySymbol propSymb)
    {
        Name = propSymb.Name;
        FullyQualifiedName = propSymb.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);

        IsByteArray = propSymb.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(byte).FullName;
        IsContractFile = propSymb.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == "Kanawanagasaki.BlazorContracts.ContractFile";
        IsReferenceType = propSymb.Type.IsReferenceType;
        IsNullable = propSymb.Type.NullableAnnotation is NullableAnnotation.Annotated;

        HasPublicSetter = propSymb.SetMethod is not null && propSymb.SetMethod.DeclaredAccessibility is Accessibility.Public;
        IsInitOnly = propSymb.SetMethod is not null && propSymb.SetMethod.IsInitOnly;

        var jsonNameAttr = propSymb.GetAttributes().FirstOrDefault(x
            => x.AttributeClass is not null
            && x.AttributeClass.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == Constants.JsonPropertyNameAttributeFullName);
        JsonPropertyName = jsonNameAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString();

        IsFormattable = propSymb.Type.AllInterfaces.Any(x => FormattableInterfaceNames.Contains(x.Name));
    }

    public string FormatToString(string accessExpression)
        => IsFormattable
            ? $"{accessExpression}.ToString(null, System.Globalization.CultureInfo.InvariantCulture)"
            : $"{accessExpression}.ToString()";
}

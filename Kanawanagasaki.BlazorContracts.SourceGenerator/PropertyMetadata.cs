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
    public bool IsRequired { get; }

    public Location? Location { get; }

    public bool IsNullableT { get; }
    public string NonNullableTypeName { get; } = string.Empty;

    public bool IsString { get; }
    public bool IsEnum { get; }
    public bool IsParsable { get; }

    public bool IsRouteParamSupported => IsString || IsEnum || IsParsable;

    public bool HasPublicSetter { get; }
    public bool IsInitOnly { get; }
    public bool HasBackingField { get; }

    public string? JsonPropertyName { get; }

    public bool IsFormattable { get; }

    private static readonly HashSet<string> FormattableInterfaceNames =
    [
        "System.IFormattable", "System.ISpanFormattable", "System.IUtf8SpanFormattable"
    ];

    public PropertyMetadata(IPropertySymbol propSymb)
    {
        Name = propSymb.Name;
        FullyQualifiedName = propSymb.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);
        Location = propSymb.Locations.FirstOrDefault();

        IsByteArray = propSymb.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(byte).FullName;
        IsContractFile = propSymb.Type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == "Kanawanagasaki.BlazorContracts.ContractFile";
        IsReferenceType = propSymb.Type.IsReferenceType;
        IsNullable = propSymb.Type.NullableAnnotation is NullableAnnotation.Annotated;
        IsRequired = propSymb.IsRequired;

        HasPublicSetter = propSymb.SetMethod is not null && propSymb.SetMethod.DeclaredAccessibility is Accessibility.Public;
        IsInitOnly = propSymb.SetMethod is not null && propSymb.SetMethod.IsInitOnly;
        HasBackingField = IsAutoProperty(propSymb);

        var jsonNameAttr = propSymb.GetAttributes().FirstOrDefault(x
            => x.AttributeClass is not null
            && x.AttributeClass.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == Constants.JsonPropertyNameAttributeFullName);
        JsonPropertyName = jsonNameAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString();

        var typeToCheck = propSymb.Type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType
            ? nullableType.TypeArguments[0]
            : propSymb.Type;

        IsFormattable = typeToCheck.AllInterfaces.Any(x => FormattableInterfaceNames.Contains(x.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT)));

        IsNullableT = propSymb.Type is INamedTypeSymbol nullablePropType
            && nullablePropType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        if (IsNullableT)
            NonNullableTypeName = typeToCheck.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);
        else
            NonNullableTypeName = FullyQualifiedName;

        IsString = typeToCheck.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(string).FullName;
        IsEnum = typeToCheck.TypeKind == TypeKind.Enum;
        if (!IsString && !IsEnum)
        {
            IsParsable = typeToCheck.AllInterfaces.Any(x
                => x.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == "System.IParsable"
                    && x.TypeArguments.Length == 1
                    && SymbolEqualityComparer.Default.Equals(x.TypeArguments[0], typeToCheck));
        }
    }

    public string FormatToString(string accessExpression)
        => IsEnum
            ? $"{accessExpression}.ToString()"
            : IsFormattable
            ? $"{accessExpression}.ToString(null, System.Globalization.CultureInfo.InvariantCulture)"
            : $"{accessExpression}.ToString()";

    private static bool IsAutoProperty(IPropertySymbol propertySymbol)
    {
        if (propertySymbol.ContainingType is null ||
            propertySymbol.IsAbstract ||
            propertySymbol.IsExtern ||
            propertySymbol.ContainingType.TypeKind == TypeKind.Interface)
        {
            return false;
        }

        if (propertySymbol.ContainingType
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Any(field => field.IsImplicitlyDeclared &&
                          SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, propertySymbol)))
        {
            return true;
        }

        return propertySymbol.DeclaringSyntaxReferences.Length == 0;
    }
}

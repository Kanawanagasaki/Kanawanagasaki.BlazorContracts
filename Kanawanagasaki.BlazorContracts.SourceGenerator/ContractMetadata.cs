namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Microsoft.CodeAnalysis;

public class ContractMetadata
{
    public string Name { get; }
    public string FullyQualifiedName { get; }

    private readonly Dictionary<string, PropertyMetadata> _properties = [];
    public IReadOnlyDictionary<string, PropertyMetadata> Properties => _properties;

    public string Endpoint { get; }
    public string VerbStr { get; }
    public bool IsReturning { get; }
    public bool IsByteArrayReturnType { get; }
    public bool IsStreamReturnType { get; }
    public bool IsDisposableReturnType { get; }
    public string? ReturnFullyQualifiedName { get; }

    private ContractMetadata(INamedTypeSymbol type, string endpoint, string verbStr)
    {
        Name = type.Name;
        FullyQualifiedName = type.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT);
        Endpoint = endpoint;
        VerbStr = verbStr;

        foreach (var prop in GetAllProperties(type))
            _properties[prop.Name] = new PropertyMetadata(prop);

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) != Constants.IContractFullName)
                continue;
            if (iface.TypeArguments.Length != 1)
                continue;

            IsReturning = true;
            ReturnFullyQualifiedName = iface.TypeArguments[0].ToDisplayString(Helper.SYMB_DISPLAY_FORMAT_GENERICS);

            if (iface.TypeArguments[0] is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(byte).FullName)
                IsByteArrayReturnType = true;
            else if (iface.TypeArguments[0].ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(Stream).FullName)
                IsStreamReturnType = true;

            if (iface.TypeArguments[0].AllInterfaces.Any(x
                => x.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == typeof(IDisposable).FullName
                || x.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == Constants.IAsyncDisposableFullName))
            {
                IsDisposableReturnType = true;
            }
        }
    }

    public static bool TryCreate(INamedTypeSymbol type, out ContractMetadata? metadata)
    {
        metadata = null;

        var attrData = GetContractAttribute(type);
        if (attrData is null)
            return false;

        var endpoint = attrData.ConstructorArguments[0].Value?.ToString();
        if (endpoint is null)
            return false;

        if (attrData.ConstructorArguments[1].Value is not int verbInt)
            return false;
        var verbStr = verbInt switch
        {
            1 => "Get",
            2 => "Post",
            3 => "Put",
            4 => "Delete",
            _ => null
        };
        if (verbStr is null)
            return false;

        metadata = new ContractMetadata(type, endpoint, verbStr);
        return metadata.VerbStr is not null && metadata.Endpoint is not null;
    }

    private static AttributeData? GetContractAttribute(INamedTypeSymbol type)
    {
        var attrData = type.GetAttributes().FirstOrDefault(
                    x => x.AttributeClass is not null
                      && x.AttributeClass.ToDisplayString(Helper.SYMB_DISPLAY_FORMAT) == Constants.ContractAttributeFullName);
        if (attrData is null)
            return null;
        if (attrData.ConstructorArguments.Length != 2)
            return null;

        return attrData;
    }

    private static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            foreach (var prop in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.DeclaredAccessibility is not Accessibility.Public)
                    continue;
                if (prop.IsStatic)
                    continue;
                if (prop.GetMethod is null)
                    continue;
                if (prop.GetMethod.DeclaredAccessibility is not Accessibility.Public)
                    continue;

                yield return prop;
            }
        }
    }
}

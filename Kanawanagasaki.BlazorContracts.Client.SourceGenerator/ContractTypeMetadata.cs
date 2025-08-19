namespace Kanawanagasaki.BlazorContracts.Client.SourceGenerator;

using Microsoft.CodeAnalysis;

internal record ContractTypeMetadata
{
    private static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT
         = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    internal string Name { get; }
    internal string FullyQualifiedName { get; }
    internal string Endpoint { get; }
    internal string Verb { get; }

    internal string[] ByteArrayProps { get; }
    internal string[] StreamProps { get; }

    internal bool IsByteArrayReturnType { get; }
    internal bool IsStreamReturnType { get; }

    internal ContractTypeMetadata(INamedTypeSymbol type, string endpoint, string verb)
    {
        Name = type.Name;
        FullyQualifiedName = type.ToDisplayString(SYMB_DISPLAY_FORMAT);
        Endpoint = endpoint;
        Verb = verb;

        var props = GetAllProperties(type);
        ByteArrayProps = props.Where(x => x.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(SYMB_DISPLAY_FORMAT) == typeof(byte).FullName)
                              .Select(x => x.Name)
                              .ToArray();
        StreamProps = props.Where(x => x.Type.ContainingNamespace + "." + x.Type.Name == "Kanawanagasaki.BlazorContracts.ContractFile")
                           .Select(x => x.Name)
                           .ToArray();

        foreach (var iface in type.AllInterfaces)
        {
            if (iface.ToDisplayString(SYMB_DISPLAY_FORMAT) != "Kanawanagasaki.BlazorContracts.IContract")
                continue;
            if (iface.TypeArguments.Length != 1)
                continue;

            if (iface.TypeArguments[0] is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(SYMB_DISPLAY_FORMAT) == typeof(byte).FullName)
                IsByteArrayReturnType = true;
            else if (iface.TypeArguments[0].ToDisplayString(SYMB_DISPLAY_FORMAT) == typeof(Stream).FullName)
                IsStreamReturnType = true;
        }
    }

    internal static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
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

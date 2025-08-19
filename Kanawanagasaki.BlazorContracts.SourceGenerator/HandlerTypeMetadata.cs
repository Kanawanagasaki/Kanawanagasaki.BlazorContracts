namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Microsoft.CodeAnalysis;

internal record HandlerTypeMetadata
{
    private static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT
         = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    internal string HandlerName { get; }
    internal string HandlerFullyQualifiedName { get; }
    internal string ContractName { get; }
    internal string ContractFullyQualifiedName { get; }
    internal string Endpoint { get; }
    internal string Verb { get; }

    internal string[] HandlerInjectedServicesTypes { get; }
    internal Dictionary<string, string> PropNameToType { get; } = new();

    internal string[] ByteArrayProps { get; }
    internal string[] StreamProps { get; }

    internal bool IsByteArrayReturnType { get; }
    internal bool IsStreamReturnType { get; }

    internal HandlerTypeMetadata(
        INamedTypeSymbol handler,
        INamedTypeSymbol contract,
        string endpoint,
        string verb)
    {
        HandlerName = handler.Name;
        HandlerFullyQualifiedName = handler.ToDisplayString(SYMB_DISPLAY_FORMAT);
        ContractName = contract.Name;
        ContractFullyQualifiedName = contract.ToDisplayString(SYMB_DISPLAY_FORMAT);
        Endpoint = endpoint;
        Verb = verb;

        var constructor = handler.Constructors.FirstOrDefault(x => x.DeclaredAccessibility is Accessibility.Public);
        HandlerInjectedServicesTypes = constructor?.Parameters.Select(x => x.Type.ToDisplayString(SYMB_DISPLAY_FORMAT)).ToArray() ?? [];

        var props = GetAllProperties(contract);
        ByteArrayProps = props.Where(x => x.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(SYMB_DISPLAY_FORMAT) == typeof(byte).FullName)
                              .Select(x => x.Name)
                              .ToArray();
        StreamProps = props.Where(x => x.Type.ContainingNamespace + "." + x.Type.Name == "Kanawanagasaki.BlazorContracts.ContractFile")
                           .Select(x => x.Name)
                           .ToArray();

        foreach (var prop in props)
            PropNameToType[prop.Name] = prop.Type.ToDisplayString(SYMB_DISPLAY_FORMAT);

        foreach (var iface in contract.AllInterfaces)
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

namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Microsoft.CodeAnalysis;

internal record HandlerTypeMetadata
{
    private static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT = new
    (
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
    );
    private static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT_GENERICS = new
    (
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );

    internal string HandlerName { get; }
    internal string HandlerFullyQualifiedName { get; }
    internal string ContractName { get; }
    internal string ContractFullyQualifiedName { get; }
    internal string Endpoint { get; }
    internal string Verb { get; }

    internal List<(string TypeName, int Index)> HandlerConstructorInjectedServicesTypes { get; } = [];
    internal List<(string TypeName, string Name, int Index)> HandlerPropertiesInjectedServicesTypes { get; } = [];
    internal List<(string TypeName, int Index)> HandlerInjectedServicesTypes { get; } = [];
    internal Dictionary<string, string> PropNameToType { get; } = new();

    internal string[] ByteArrayProps { get; }
    internal string[] StreamProps { get; }

    internal bool IsContractWithResponse { get; }
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

        int injectedServiceIndex = 1;

        var constructor = handler.Constructors.FirstOrDefault(x => x.DeclaredAccessibility is Accessibility.Public);
        var constructorTypes = constructor?.Parameters.Select(x => x.Type.ToDisplayString(SYMB_DISPLAY_FORMAT_GENERICS)).ToArray() ?? [];
        foreach (var type in constructorTypes)
        {
            int index = injectedServiceIndex++;
            HandlerConstructorInjectedServicesTypes.Add((type, index));
            HandlerInjectedServicesTypes.Add((type, index));
        }

        var handlerProps = GetAllProperties(handler);
        var handlerPropTypes = handlerProps.Select(x => (TypeName: x.Type.ToDisplayString(SYMB_DISPLAY_FORMAT_GENERICS), Name: x.Name)).ToArray();
        foreach (var (type, name) in handlerPropTypes)
        {
            int index = injectedServiceIndex++;
            HandlerPropertiesInjectedServicesTypes.Add((type, name, index));
            HandlerInjectedServicesTypes.Add((type, index));
        }

        var contractProps = GetAllProperties(contract);
        ByteArrayProps = contractProps.Where(x => x.Type is IArrayTypeSymbol arr && arr.ElementType.ToDisplayString(SYMB_DISPLAY_FORMAT) == typeof(byte).FullName)
                              .Select(x => x.Name)
                              .ToArray();
        StreamProps = contractProps.Where(x => x.Type.ContainingNamespace + "." + x.Type.Name == "Kanawanagasaki.BlazorContracts.ContractFile")
                           .Select(x => x.Name)
                           .ToArray();

        foreach (var prop in contractProps)
            PropNameToType[prop.Name] = prop.Type.ToDisplayString(SYMB_DISPLAY_FORMAT_GENERICS);

        foreach (var iface in contract.AllInterfaces)
        {
            if (iface.ToDisplayString(SYMB_DISPLAY_FORMAT) != "Kanawanagasaki.BlazorContracts.IContract")
                continue;
            if (iface.TypeArguments.Length != 1)
                continue;

            IsContractWithResponse = true;

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

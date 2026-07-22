namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Kanawanagasaki.BlazorContracts.SourceGenerator.Shared;
using Microsoft.CodeAnalysis;

internal record HandlerMetadata
{
    internal string HandlerName { get; }
    internal string HandlerFullyQualifiedName { get; }
    internal ContractMetadata Contract { get; }

    internal List<(string TypeName, int Index)> HandlerConstructorInjectedServicesTypes { get; } = [];
    internal List<(string TypeName, string Name, int Index)> HandlerPropertiesInjectedServicesTypes { get; } = [];
    internal List<(string TypeName, int Index)> HandlerInjectedServicesTypes { get; } = [];

    private HandlerMetadata(INamedTypeSymbol handler, ContractMetadata contract)
    {
        HandlerName = handler.Name;
        HandlerFullyQualifiedName = handler.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT);
        Contract = contract;

        int injectedServiceIndex = 1;

        var constructor = handler.Constructors.FirstOrDefault(x => x.DeclaredAccessibility is Accessibility.Public);
        var constructorTypes = constructor?.Parameters.Select(x => x.Type.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT_GENERICS)).ToArray() ?? [];
        foreach (var type in constructorTypes)
        {
            int index = injectedServiceIndex++;
            HandlerConstructorInjectedServicesTypes.Add((type, index));
            HandlerInjectedServicesTypes.Add((type, index));
        }

        var handlerProps = GetAllProperties(handler);
        var handlerPropTypes = handlerProps.Select(x => (TypeName: x.Type.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT_GENERICS), x.Name)).ToArray();
        foreach (var (type, name) in handlerPropTypes)
        {
            int index = injectedServiceIndex++;
            HandlerPropertiesInjectedServicesTypes.Add((type, name, index));
            HandlerInjectedServicesTypes.Add((type, index));
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

    internal static bool TryCreate(INamedTypeSymbol handler, out HandlerMetadata? metadata)
    {
        metadata = null;

        var contractHandlerIFace = handler.AllInterfaces.FirstOrDefault(x => x.ToDisplayString(NamingHelper.SYMB_DISPLAY_FORMAT) == Constants.IContractHandlerFullName);
        if (contractHandlerIFace is null)
            return false;
        if (contractHandlerIFace.TypeArguments.Length == 0)
            return false;

        var contractType = contractHandlerIFace.TypeArguments[0];
        if (contractType is not INamedTypeSymbol contract)
            return false;
        if (!ContractMetadata.TryCreate(contract, out var contractMetadata))
            return false;
        if (contractMetadata is null)
            return false;

        metadata = new HandlerMetadata(handler, contractMetadata);
        return true;
    }
}

namespace Kanawanagasaki.BlazorContracts.SourceGenerator;

using Microsoft.CodeAnalysis;

internal static class Helper
{
    internal static bool IsFrameworkAssembly(string name)
        => name.StartsWith("System") ||
           name.StartsWith("Microsoft") ||
           name.StartsWith("mscorlib") ||
           name.StartsWith("netstandard");

    internal static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol, CancellationToken ct)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            ct.ThrowIfCancellationRequested();

            if (member is INamespaceSymbol childNamespace)
            {
                foreach (var type in GetAllTypes(childNamespace, ct))
                    yield return type;
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;

                foreach (var nested in GetNestedTypes(type, ct))
                    yield return nested;
            }
        }
    }

    internal static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type, CancellationToken ct)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            ct.ThrowIfCancellationRequested();

            yield return nested;
            foreach (var deep in GetNestedTypes(nested, ct))
                yield return deep;
        }
    }

    internal static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT
         = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    internal static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT_GENERICS = new
    (
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );
}

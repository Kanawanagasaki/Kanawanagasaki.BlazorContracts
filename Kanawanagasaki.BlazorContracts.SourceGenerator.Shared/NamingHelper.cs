namespace Kanawanagasaki.BlazorContracts.SourceGenerator.Shared;

using Microsoft.CodeAnalysis;

public static class NamingHelper
{
    public static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT
         = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    public static readonly SymbolDisplayFormat SYMB_DISPLAY_FORMAT_GENERICS = new
    (
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
    );
}

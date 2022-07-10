using Microsoft.CodeAnalysis;

namespace RichEntity.Extensions;

public static class SymbolExtensions
{
    public static IEnumerable<ISymbol> WhereSymbolsArePropertyFields(this IEnumerable<ISymbol> symbols)
        => symbols.Where(s => s.Kind is SymbolKind.Property || s.Kind is SymbolKind.Field);

    public static bool ContainsPropertyFieldCalled(this IEnumerable<ISymbol> symbols, string name)
        => symbols.WhereSymbolsArePropertyFields().Any(s => s.Name.Equals(name));

    public static bool EqualsDefault(this ISymbol left, ISymbol? right)
        => left.Equals(right, SymbolEqualityComparer.Default);

    public static bool HasAttribute(this ISymbol symbol, ITypeSymbol? attribute)
        => symbol.GetAttributes().Any(a => attribute?.EqualsDefault(a.AttributeClass) ?? false);
}
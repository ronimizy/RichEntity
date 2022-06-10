using Microsoft.CodeAnalysis;

namespace RichEntity.Analysis.Extensions;

public static class SymbolExtensions
{
    public static IEnumerable<ISymbol> WhereSymbolsArePropertyFields(this IEnumerable<ISymbol> symbols)
        => symbols.Where(s => s.Kind is SymbolKind.Property || s.Kind is SymbolKind.Field);

    public static bool ContainsPropertyFieldCalled(this IEnumerable<ISymbol> symbols, string name)
        => symbols.WhereSymbolsArePropertyFields().Any(s => s.Name.Equals(name));
}
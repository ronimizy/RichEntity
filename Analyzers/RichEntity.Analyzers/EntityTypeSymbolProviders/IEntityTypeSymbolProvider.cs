using Microsoft.CodeAnalysis;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders
{
    public interface IEntityTypeSymbolProvider
    {
        bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol);
    }
}
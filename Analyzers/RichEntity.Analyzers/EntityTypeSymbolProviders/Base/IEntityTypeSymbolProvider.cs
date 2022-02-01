using Microsoft.CodeAnalysis;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders.Base
{
    public interface IEntityTypeSymbolProvider
    {
        bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol);
    }
}
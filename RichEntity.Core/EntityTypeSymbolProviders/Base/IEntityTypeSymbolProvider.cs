using Microsoft.CodeAnalysis;

namespace RichEntity.Core.EntityTypeSymbolProviders.Base
{
    public interface IEntityTypeSymbolProvider
    {
        bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol);
    }
}
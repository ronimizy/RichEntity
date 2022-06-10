using Microsoft.CodeAnalysis;

namespace RichEntity.Analysis.EntityTypeSymbolProviders.Base;

public interface IEntityTypeSymbolProvider
{
    bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol);
}
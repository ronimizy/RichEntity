using Microsoft.CodeAnalysis;

namespace RichEntity.EntityFrameworkCore.Analysis.EntityTypeSymbolProviders.Base;

public interface IEntityTypeSymbolProvider
{
    bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol);
}
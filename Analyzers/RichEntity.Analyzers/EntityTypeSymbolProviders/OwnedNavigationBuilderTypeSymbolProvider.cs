using Microsoft.CodeAnalysis;
using RichEntity.Analyzers.EntityTypeSymbolProviders.Base;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders
{
    public class OwnedNavigationBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
    {
        protected override string FullyQualifiedName =>
            "Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder";

        protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
            => designatedTypeSymbol.TypeArguments[0];
    }
}
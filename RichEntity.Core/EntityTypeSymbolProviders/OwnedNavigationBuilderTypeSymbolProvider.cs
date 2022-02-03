using Microsoft.CodeAnalysis;
using RichEntity.Core.EntityTypeSymbolProviders.Base;

namespace RichEntity.Core.EntityTypeSymbolProviders
{
    public class OwnedNavigationBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
    {
        protected override string FullyQualifiedName =>
            "Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder";

        protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
            => designatedTypeSymbol.TypeArguments[0];
    }
}
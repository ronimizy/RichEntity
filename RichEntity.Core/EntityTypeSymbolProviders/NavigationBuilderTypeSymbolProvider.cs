using Microsoft.CodeAnalysis;
using RichEntity.Core.EntityTypeSymbolProviders.Base;

namespace RichEntity.Core.EntityTypeSymbolProviders
{
    public class NavigationBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
    {
        protected override string FullyQualifiedName =>
            "Microsoft.EntityFrameworkCore.Metadata.Builders.NavigationBuilder";

        protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
            => designatedTypeSymbol.TypeArguments[0];
    }
}
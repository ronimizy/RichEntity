using System.Linq;
using Microsoft.CodeAnalysis;
using RichEntity.Analyzers.EntityTypeSymbolProviders.Base;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders
{
    public class EntityBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
    {
        protected override string FullyQualifiedName =>
            "Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder";

        protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
            => designatedTypeSymbol.TypeArguments.Single();
    }
}
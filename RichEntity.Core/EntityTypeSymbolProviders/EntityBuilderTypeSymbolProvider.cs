using System.Linq;
using Microsoft.CodeAnalysis;
using RichEntity.Core.EntityTypeSymbolProviders.Base;

namespace RichEntity.Core.EntityTypeSymbolProviders
{
    public class EntityBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
    {
        protected override string FullyQualifiedName =>
            "Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder";

        protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
            => designatedTypeSymbol.TypeArguments.Single();
    }
}
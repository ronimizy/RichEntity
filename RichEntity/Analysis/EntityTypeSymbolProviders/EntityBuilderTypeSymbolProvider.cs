using Microsoft.CodeAnalysis;
using RichEntity.Analysis.EntityTypeSymbolProviders.Base;

namespace RichEntity.Analysis.EntityTypeSymbolProviders;

public class EntityBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
{
    protected override string FullyQualifiedName =>
        "Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder";

    protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
        => designatedTypeSymbol.TypeArguments.Single();
}
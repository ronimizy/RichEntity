using Microsoft.CodeAnalysis;
using RichEntity.EntityFrameworkCore.Analysis.EntityTypeSymbolProviders.Base;

namespace RichEntity.EntityFrameworkCore.Analysis.EntityTypeSymbolProviders;

public class NavigationBuilderTypeSymbolProvider : EntityBuilderTypeSymbolProviderBase
{
    protected override string FullyQualifiedName =>
        "Microsoft.EntityFrameworkCore.Metadata.Builders.NavigationBuilder";

    protected override ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol)
        => designatedTypeSymbol.TypeArguments[0];
}
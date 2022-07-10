using FluentChaining;
using Microsoft.CodeAnalysis;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;

namespace RichEntity.Generation.Entity.Extensions;

public static class GetIdentifierExtensions
{
    public static IEnumerable<Identifier> ProcessOrEmpty(
        this IChain<GetIdentifiersCommand, IEnumerable<Identifier>> chain,
        INamedTypeSymbol? symbol,
        Compilation compilation)
    {
        if (symbol is null)
            return Enumerable.Empty<Identifier>();

        var request = new GetIdentifiersCommand(symbol, compilation);
        return chain.Process(request);
    }
}
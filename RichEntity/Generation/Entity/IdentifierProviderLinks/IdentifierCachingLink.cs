using System.Collections.Concurrent;
using FluentChaining;
using Microsoft.CodeAnalysis;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;

namespace RichEntity.Generation.Entity.IdentifierProviderLinks;

public class IdentifierCachingLink : ILink<GetIdentifiersCommand, IEnumerable<Identifier>>
{
    private readonly ConcurrentDictionary<INamedTypeSymbol, IReadOnlyCollection<Identifier>> _cache;

    public IdentifierCachingLink()
    {
        _cache = new ConcurrentDictionary<INamedTypeSymbol, IReadOnlyCollection<Identifier>>(
            SymbolEqualityComparer.Default);
    }

    public IEnumerable<Identifier> Process(
        GetIdentifiersCommand request,
        SynchronousContext context,
        LinkDelegate<GetIdentifiersCommand, SynchronousContext, IEnumerable<Identifier>> next)
    {
        return _cache.GetOrAdd(request.Symbol, _ => next.Invoke(request, context).ToArray());
    }
}
using FluentChaining;
using Microsoft.CodeAnalysis;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;
using RichEntity.Utility;

namespace RichEntity.Generation.Entity.IdentifierProviderLinks;

public class KeyPropertyIdentifierProvider : ILink<GetIdentifiersCommand, IEnumerable<Identifier>>
{
    public IEnumerable<Identifier> Process(
        GetIdentifiersCommand request,
        SynchronousContext context,
        LinkDelegate<GetIdentifiersCommand, SynchronousContext, IEnumerable<Identifier>> next)
    {
        IEnumerable<Identifier> identifiers = next.Invoke(request, context);

        var entityInterface = request.Compilation
            .GetTypeByMetadataName(Constants.CompositeEntityInterfaceFullyQualifiedName);

        if (entityInterface is null)
            return identifiers;

        if (!request.Symbol.AllInterfaces.Any(i => i.IsAssignableTo(entityInterface)))
            return identifiers;

        var genericEntityInterface = request.Compilation
            .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName);

        if (genericEntityInterface is null)
            return identifiers;

        var attribute = request.Compilation
            .GetTypeByMetadataName(Constants.KeyEntityAttributeFullyQualifiedName);

        if (attribute is null)
            return identifiers;

        IEnumerable<Identifier> keyEntities = GetSymbolsFromAllHierarchy(request.Symbol)
            .Reverse()
            .OfType<IPropertySymbol>()
            .Where(s => s.HasAttribute(attribute))
            .SelectMany(s => SymbolToIdentifiers(s, genericEntityInterface, entityInterface, attribute));

        return keyEntities.Concat(identifiers);
    }

    private static IEnumerable<ISymbol> GetSymbolsFromAllHierarchy(ITypeSymbol? symbol)
    {
        while (symbol is not null)
        {
            foreach (var member in symbol.GetMembers())
            {
                yield return member;
            }

            symbol = symbol.BaseType;
        }
    }

    private static IEnumerable<Identifier> SymbolToIdentifiers(
        IPropertySymbol symbol,
        INamedTypeSymbol genericEntityInterface,
        INamedTypeSymbol entityInterface,
        INamedTypeSymbol keyPropertyAttribute)
    {
        if (symbol.Type.IsAssignableTo(entityInterface))
        {
            return GetSymbolsFromAllHierarchy(symbol.Type)
                .Where(x => x.HasAttribute(keyPropertyAttribute))
                .OfType<IPropertySymbol>()
                .SelectMany(x => SymbolToIdentifiers(x, genericEntityInterface, entityInterface, keyPropertyAttribute));
        }

        return Enumerable.Repeat(SymbolToIdentifier(symbol, genericEntityInterface), 1);
    }

    private static Identifier SymbolToIdentifier(IPropertySymbol symbol, INamedTypeSymbol genericEntityInterface)
    {
        string capitalized;
        string lowercased;
        ITypeSymbol identifierType;

        if (symbol.Type.IsAssignableTo(genericEntityInterface))
        {
            capitalized = $"{char.ToUpper(symbol.Name[0])}{symbol.Name.Substring(1)}Id";
            lowercased = $"{char.ToLower(symbol.Name[0])}{symbol.Name.Substring(1)}Id";

            var concreteInterface = symbol.Type.AllInterfaces
                .Single(i => i.IsAssignableTo(genericEntityInterface));

            identifierType = concreteInterface.TypeArguments.Single();
        }
        else
        {
            capitalized = $"{char.ToUpper(symbol.Name[0])}{symbol.Name.Substring(1)}";
            lowercased = $"{char.ToLower(symbol.Name[0])}{symbol.Name.Substring(1)}";
            identifierType = symbol.Type;
        }

        return new Identifier(capitalized, lowercased, identifierType);
    }
}
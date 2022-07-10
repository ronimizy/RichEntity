using FluentChaining;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;
using RichEntity.Utility;

namespace RichEntity.Generation.Entity.IdentifierProviderLinks;

public class EntityIdentifierProviderLink : ILink<GetIdentifiersCommand, IEnumerable<Identifier>>
{
    public IEnumerable<Identifier> Process(
        GetIdentifiersCommand request,
        SynchronousContext context,
        LinkDelegate<GetIdentifiersCommand, SynchronousContext, IEnumerable<Identifier>> next)
    {
        IEnumerable<Identifier> nextIdentifiers = next(request, context);
        
        var entityInterface = request.Compilation
            .GetTypeByMetadataName(Constants.EntityInterfaceFullyQualifiedName);

        if (entityInterface is null)
            return nextIdentifiers;

        var concreteEntityInterface = request.Symbol.AllInterfaces
            .SingleOrDefault(t => t.IsAssignableTo(entityInterface));

        if (concreteEntityInterface?.TypeArguments.Length is not 1)
            return nextIdentifiers;

        var identifier = new Identifier("Id", "id", concreteEntityInterface.TypeArguments[0]);

        return nextIdentifiers.Append(identifier);
    }
}
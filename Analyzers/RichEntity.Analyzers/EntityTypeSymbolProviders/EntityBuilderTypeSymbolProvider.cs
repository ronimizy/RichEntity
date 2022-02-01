using System.Linq;
using Microsoft.CodeAnalysis;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders
{
    public class EntityBuilderTypeSymbolProvider : IEntityTypeSymbolProvider
    {
        private const string FullyQualifiedName = "Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder";

        public bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol)
        {
            var entityBuilderType = compilation.GetTypeByMetadataName(FullyQualifiedName);
            if (operation.Kind is OperationKind.ParameterReference &&
                operation.Type is INamedTypeSymbol namedTypeSymbol &&
                (namedTypeSymbol.BaseType?.Equals(entityBuilderType) ?? false))
            {
                symbol = namedTypeSymbol.TypeArguments.Single();
                return true;
            }

            symbol = null;
            return false;
        }
    }
}
using Microsoft.CodeAnalysis;
using RichEntity.Analyzers.Extensions;

namespace RichEntity.Analyzers.EntityTypeSymbolProviders.Base
{
    public abstract class EntityBuilderTypeSymbolProviderBase : IEntityTypeSymbolProvider
    {
        protected abstract string FullyQualifiedName { get; }
        
        public bool TryGetTypeSymbol(IOperation operation, Compilation compilation, out ITypeSymbol? symbol)
        {
            var navigationBuilderType = compilation.GetTypeByMetadataName(FullyQualifiedName);
            symbol = null;

            if (navigationBuilderType is null)
                return false;

            if (operation.Type is INamedTypeSymbol namedTypeSymbol &&
                namedTypeSymbol.DerivesFrom(navigationBuilderType))
            {
                var unwrapped = namedTypeSymbol.UnwrapToFirstDerivative(navigationBuilderType);

                if (unwrapped is null)
                    return false;
                
                symbol = ExtractType(unwrapped);
                return true;
            }

            return false;
        }

        protected abstract ITypeSymbol ExtractType(INamedTypeSymbol designatedTypeSymbol);
    }
}
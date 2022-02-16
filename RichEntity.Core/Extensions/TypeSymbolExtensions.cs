using Microsoft.CodeAnalysis;

namespace RichEntity.Core.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static bool DerivesFrom(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol baseClass)
        {
            var derivative = namedTypeSymbol;

            while (derivative is { })
            {
                if (derivative.Equals(baseClass))
                    return true;

                derivative = derivative.BaseType;
            }

            return false;
        }

        public static bool DerivesOrConstructedFrom(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol baseType)
        {
            if (namedTypeSymbol.EqualsDefault(baseType))
                return true;

            if (namedTypeSymbol.ConstructedFrom.EqualsDefault(baseType))
                return true;

            if (namedTypeSymbol.BaseType is object && namedTypeSymbol.BaseType.DerivesOrConstructedFrom(baseType))
                return true;

            if (!namedTypeSymbol.ConstructedFrom.EqualsDefault(namedTypeSymbol) &&
                namedTypeSymbol.ConstructedFrom.DerivesOrConstructedFrom(baseType))
                return true;

            return false;
        }

        public static INamedTypeSymbol? UnwrapToFirstDerivative(
            this INamedTypeSymbol? unwrapped, INamedTypeSymbol derivationSource)
        {
            while (unwrapped is { } && !derivationSource.Equals(unwrapped.BaseType))
            {
                unwrapped = unwrapped.BaseType;
            }

            return unwrapped;
        }

        public static bool EqualsDefault(this ITypeSymbol left, ITypeSymbol right)
            => left.Equals(right, SymbolEqualityComparer.Default);
    }
}
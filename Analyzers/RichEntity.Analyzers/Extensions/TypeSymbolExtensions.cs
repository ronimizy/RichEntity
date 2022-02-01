using Microsoft.CodeAnalysis;

namespace RichEntity.Analyzers.Extensions
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

        public static INamedTypeSymbol? UnwrapToFirstDerivative(
            this INamedTypeSymbol? unwrapped, INamedTypeSymbol derivationSource)
        {
            while (unwrapped is { } && !derivationSource.Equals(unwrapped.BaseType))
            {
                unwrapped = unwrapped.BaseType;
            }

            return unwrapped;
        }
    }
}
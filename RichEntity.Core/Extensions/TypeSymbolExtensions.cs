using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RichEntity.Core.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static bool DerivesFrom(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol baseClass)
        {
            var derivative = namedTypeSymbol;

            while (derivative is object)
            {
                if (derivative.EqualsProperly(baseClass))
                    return true;

                derivative = derivative.BaseType;
            }

            return false;
        }

        public static INamedTypeSymbol? UnwrapToFirstDerivative(
            this INamedTypeSymbol? unwrapped, INamedTypeSymbol derivationSource)
        {
            while (unwrapped is object && !derivationSource.EqualsProperly(unwrapped.BaseType))
            {
                unwrapped = unwrapped.BaseType;
            }

            return unwrapped;
        }


        public static ImmutableArray<ISymbol> GetAllMembers(this INamedTypeSymbol? typeSymbol)
        {
            var members = new List<ISymbol>();

            while (typeSymbol is object)
            {
                members.AddRange(typeSymbol.GetMembers());
                typeSymbol = typeSymbol.BaseType;
            }

            return members.ToImmutableArray();
        }
    }
}
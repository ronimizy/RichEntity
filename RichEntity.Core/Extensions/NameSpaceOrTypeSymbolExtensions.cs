using System.Text;
using Microsoft.CodeAnalysis;

namespace RichEntity.Core.Extensions
{
    public static class NameSpaceOrTypeSymbolExtensions
    {
        public static string GetFullyQualifiedName(this INamespaceOrTypeSymbol symbol)
        {
            var builder = new StringBuilder(symbol.Name);
            symbol = symbol.ContainingType ?? (INamespaceOrTypeSymbol)symbol.ContainingNamespace;

            while (symbol is { } && !(symbol is INamespaceSymbol { IsGlobalNamespace: true }))
            {
                builder.Insert(0, '.');
                builder.Insert(0, symbol.Name);
                symbol = symbol.ContainingType ?? (INamespaceOrTypeSymbol)symbol.ContainingNamespace;
            }

            return builder.ToString();
        }
    }
}
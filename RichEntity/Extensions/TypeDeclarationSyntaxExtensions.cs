using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Extensions;

public static class TypeDeclarationSyntaxExtensions
{
    public static AttributeSyntax GetAttributeSyntax(
        this TypeDeclarationSyntax syntax,
        INamedTypeSymbol attribute,
        Compilation compilation)
    {
        bool SyntaxPredicate(AttributeSyntax a)
        {
            var s = compilation
                .GetSemanticModel(a.SyntaxTree)
                .GetSymbolInfo(a)
                .Symbol?.ContainingSymbol;

            return attribute.EqualsDefault(s);
        }

        return syntax.AttributeLists
            .SelectMany(l => l.Attributes)
            .Single(SyntaxPredicate);
    }
}
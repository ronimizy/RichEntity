using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Extensions;

public static class ArgumentDeclarationSyntaxExtensions
{
    public static AttributeArgumentSyntax? GetArgumentOrDefault(this AttributeSyntax syntax, string name)
        => syntax.ArgumentList?.Arguments.SingleOrDefault(a => name.Equals(a.NameEquals?.Name.ToString()));
}
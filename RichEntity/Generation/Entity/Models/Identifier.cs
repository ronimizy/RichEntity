using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.Models;

public record Identifier(string CapitalizedName, string LowercasedName, ITypeSymbol Type) : IEquatable<IPropertySymbol>
{
    public virtual ArgumentSyntax GetArgument(bool capitalized = true)
        => Argument(IdentifierName(capitalized ? CapitalizedName : LowercasedName));

    public virtual ParameterSyntax GetParameter(bool capitalized = true)
    {
        return Parameter(Identifier(capitalized ? CapitalizedName : LowercasedName))
            .WithType(IdentifierName(Type.Name));
    }

    public virtual IdentifierNameSyntax GetIdentifierName(bool capitalized = true)
        => IdentifierName(capitalized ? CapitalizedName : LowercasedName);

    public virtual SyntaxToken GetIdentifier(bool capitalized = true)
        => Identifier(capitalized ? CapitalizedName : LowercasedName);

    public virtual IdentifierNameSyntax GetTypeIdentifierName()
        => IdentifierName(Type.Name);

    public bool Equals(IPropertySymbol? symbol)
    {
        if (symbol is null)
            return false;
        
        return symbol.Name.Equals(CapitalizedName) && symbol.Type.EqualsDefault(Type);
    }
}
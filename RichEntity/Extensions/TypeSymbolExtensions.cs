using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Extensions;

public static class TypeSymbolExtensions
{
    public static bool IsAssignableTo(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol baseType)
    {
        if (namedTypeSymbol.EqualsDefault(baseType))
            return true;

        if (namedTypeSymbol.ConstructedFrom.EqualsDefault(baseType))
            return true;

        if (namedTypeSymbol.BaseType is not null && namedTypeSymbol.BaseType.IsAssignableTo(baseType))
            return true;

        if (!namedTypeSymbol.ConstructedFrom.EqualsDefault(namedTypeSymbol) &&
            namedTypeSymbol.ConstructedFrom.IsAssignableTo(baseType))
            return true;

        return false;
    }

    public static bool DerivesFrom(this INamedTypeSymbol namedTypeSymbol, INamedTypeSymbol baseClass)
    {
        var derivative = namedTypeSymbol;

        while (derivative is { })
        {
            if (derivative.EqualsDefault(baseClass))
                return true;

            derivative = derivative.BaseType;
        }

        return false;
    }

    public static INamedTypeSymbol? UnwrapToFirstDerivative(
        this INamedTypeSymbol? unwrapped,
        INamedTypeSymbol derivationSource)
    {
        while (unwrapped is { } && !derivationSource.EqualsDefault(unwrapped.BaseType))
        {
            unwrapped = unwrapped.BaseType;
        }

        return unwrapped;
    }

    public static TypeDeclarationSyntax ToSyntax(this INamedTypeSymbol symbol)
    {
        return symbol.TypeKind switch
        {
            TypeKind.Class when symbol.IsRecord => RecordDeclaration(Token(SyntaxKind.RecordKeyword), symbol.Name)
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken)),

            TypeKind.Class => ClassDeclaration(symbol.Name),

            TypeKind.Struct when symbol.IsRecord => RecordDeclaration(Token(SyntaxKind.RecordKeyword), symbol.Name)
                .WithClassOrStructKeyword(Token(SyntaxKind.StructKeyword))
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken)),

            TypeKind.Struct => StructDeclaration(symbol.Name),

            TypeKind.Interface => InterfaceDeclaration(symbol.Name),

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static bool HasAttribute(this ITypeSymbol symbol, ITypeSymbol? attribute)
        => symbol.GetAttributes().Any(a => attribute?.EqualsDefault(a.AttributeClass) ?? false);
}
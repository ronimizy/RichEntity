using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Extensions;

public static class TypeSymbolExtensions
{
    public static bool IsAssignableTo(this ITypeSymbol typeSymbol, ITypeSymbol baseType)
    {
        if (typeSymbol.EqualsDefault(baseType))
            return true;

        if (typeSymbol.BaseType is not null && typeSymbol.BaseType.IsAssignableTo(baseType))
            return true;

        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
            return false;

        if (namedTypeSymbol.ConstructedFrom.EqualsDefault(baseType))
            return true;

        if (!namedTypeSymbol.ConstructedFrom.EqualsDefault(typeSymbol) &&
            namedTypeSymbol.ConstructedFrom.IsAssignableTo(baseType))
            return true;

        return typeSymbol.AllInterfaces.Any(s => s.IsAssignableTo(baseType));
    }

    public static bool DerivesFrom(this ITypeSymbol namedTypeSymbol, ITypeSymbol baseClass)
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
}
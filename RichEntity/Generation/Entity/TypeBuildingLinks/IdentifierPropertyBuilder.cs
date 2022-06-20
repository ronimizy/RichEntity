using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Annotations;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = RichEntity.Annotations.Accessibility;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class IdentifierPropertyBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly AccessorDeclarationSyntax GetAccessor;
    private static readonly SyntaxToken PropertyNameIdentifier;
    private static readonly SyntaxTokenList Modifiers;

    static IdentifierPropertyBuilder()
    {
        GetAccessor = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        PropertyNameIdentifier = Identifier("Id");
        Modifiers = TokenList(Token(SyntaxKind.PublicKeyword));
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var identifierName = IdentifierName(request.IdentifierSymbol.Name);
        var setAccessor = GetSetAccessor(request);

        var accessors = AccessorList(List(new[]
        {
            GetAccessor,
            setAccessor,
        }));

        var property = PropertyDeclaration(identifierName, PropertyNameIdentifier)
            .WithModifiers(Modifiers)
            .WithAccessorList(accessors);

        request = request with
        {
            Root = request.Root.AddMembers(property)
        };

        return next(request, context);
    }

    private static AccessorDeclarationSyntax GetSetAccessor(TypeBuildingCommand request)
    {
        var setterAccessModifier = GetSetterAccessModifier(request);
        var accessorKind = GetSetterKind(request);

        var setAccessor = AccessorDeclaration(accessorKind)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        if (setterAccessModifier is not null)
        {
            setAccessor = setAccessor.WithModifiers(TokenList(Token(setterAccessModifier.Value)));
        }

        return setAccessor;
    }

    private static SyntaxKind? GetSetterAccessModifier(TypeBuildingCommand request)
    {
        SyntaxKind? modifier = request.Symbol.TypeKind is TypeKind.Struct
            ? SyntaxKind.PrivateKeyword
            : SyntaxKind.ProtectedKeyword;

        var attribute = request.Compilation
            .GetTypeByMetadataName(Constants.ConfigureIdAttributeFullyQualifiedName);

        if (attribute is null || !request.Symbol.HasAttribute(attribute))
            return modifier;

        var syntax = request.Syntax.GetAttributeSyntax(attribute, request.Compilation);
        var argument = syntax.GetArgumentOrDefault(nameof(ConfigureIdAttribute.SetterAccessibility));
        var value = (argument?.Expression as MemberAccessExpressionSyntax)?.Name.ToString();

        return value switch
        {
            nameof(Accessibility.Private) => SyntaxKind.PrivateKeyword,
            nameof(Accessibility.Protected) => SyntaxKind.ProtectedKeyword,
            nameof(Accessibility.Internal) => SyntaxKind.InternalKeyword,
            nameof(Accessibility.Public) => null,
            _ => modifier,
        };
    }

    private static SyntaxKind GetSetterKind(TypeBuildingCommand request)
    {
        var accessorKind = SyntaxKind.InitAccessorDeclaration;

        var attribute = request.Compilation
            .GetTypeByMetadataName(Constants.ConfigureIdAttributeFullyQualifiedName);

        if (attribute is null || !request.Symbol.HasAttribute(attribute))
            return accessorKind;

        var syntax = request.Syntax.GetAttributeSyntax(attribute, request.Compilation);
        var argument = syntax.GetArgumentOrDefault(nameof(ConfigureIdAttribute.SetterType));
        var value = (argument?.Expression as MemberAccessExpressionSyntax)?.Name.ToString();

        return value switch
        {
            nameof(SetterType.Init) => SyntaxKind.InitAccessorDeclaration,
            nameof(SetterType.Set) => SyntaxKind.SetAccessorDeclaration,
            _ => accessorKind
        };
    }
}
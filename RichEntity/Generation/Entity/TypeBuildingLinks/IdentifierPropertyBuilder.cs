using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Annotations;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Extensions;
using RichEntity.Generation.Entity.Models;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = RichEntity.Annotations.Accessibility;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class IdentifierPropertyBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly AccessorDeclarationSyntax GetAccessor;
    private static readonly SyntaxTokenList Modifiers;
    
    private readonly IChain<GetIdentifiersCommand, IEnumerable<Identifier>> _getIdentifiersChain;

    static IdentifierPropertyBuilder()
    {
        GetAccessor = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        Modifiers = TokenList(Token(SyntaxKind.PublicKeyword));
    }

    public IdentifierPropertyBuilder(IChain<GetIdentifiersCommand, IEnumerable<Identifier>> getIdentifiersChain)
    {
        _getIdentifiersChain = getIdentifiersChain;
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var setAccessor = GetSetAccessor(request);

        var accessors = AccessorList(List(new[]
        {
            GetAccessor,
            setAccessor,
        }));

        Identifier[] baseIdentifiers = _getIdentifiersChain
            .ProcessOrEmpty(request.Symbol.BaseType, request.Compilation)
            .ToArray();

        MemberDeclarationSyntax[] properties = request.Identifiers
            .Where(i => !baseIdentifiers.Contains(i))
            .Select(i => BuidlIdentifierProperty(i, accessors))
            .ToArray();

        request = request with
        {
            Root = request.Root.AddMembers(properties)
        };

        return next(request, context);
    }

    private static MemberDeclarationSyntax BuidlIdentifierProperty(Identifier identifier, AccessorListSyntax accessors)
    {
        var type = IdentifierName(identifier.Type.Name);
        var name = Identifier(identifier.CapitalizedName);

        return PropertyDeclaration(type, name)
            .WithModifiers(Modifiers)
            .WithAccessorList(accessors);
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
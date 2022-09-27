using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Annotations;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Extensions;
using RichEntity.Generation.Entity.Models;
using RichEntity.ObjectCreation;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

        IEnumerable<Identifier> baseIdentifiers = _getIdentifiersChain
            .ProcessOrEmpty(request.Symbol.BaseType, request.Compilation);

        IPropertySymbol[] members = request.Symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .ToArray();

        IEnumerable<Identifier> alreadyImplementedIdentifiers = request.Identifiers
            .Where(i => members.Any(i.Equals));

        MemberDeclarationSyntax[] properties = request.Identifiers
            .Except(baseIdentifiers)
            .Except(alreadyImplementedIdentifiers)
            .Select(i => BuildIdentifierProperty(i, accessors))
            .ToArray();

        request = request with
        {
            Root = request.Root.AddMembers(properties)
        };

        return next(request, context);
    }

    private static MemberDeclarationSyntax BuildIdentifierProperty(Identifier identifier, AccessorListSyntax accessors)
    {
        return PropertyDeclaration(identifier.GetTypeIdentifierName(), identifier.GetIdentifier())
            .WithModifiers(Modifiers)
            .WithAccessorList(accessors);
    }

    private AccessorDeclarationSyntax GetSetAccessor(TypeBuildingCommand request)
    {
        ConfigureIdAttribute? idConfiguration = null;

        var attribute = request.Compilation
            .GetTypeByMetadataName(Constants.ConfigureIdAttributeFullyQualifiedName);

        if (attribute is not null && request.Symbol.HasAttribute(attribute))
        {
            var syntax = request.Syntax.GetAttributeSyntax(attribute, request.Compilation);
            idConfiguration = ObjectCreator.Create<ConfigureIdAttribute>(syntax, request.Compilation);
        }

        SyntaxKind? setterAccessModifier = GetSetterAccessModifier(request, idConfiguration);
        SyntaxKind accessorKind = GetSetterKind(idConfiguration);

        var setAccessor = AccessorDeclaration(accessorKind)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        if (setterAccessModifier is not null)
        {
            setAccessor = setAccessor.WithModifiers(TokenList(Token(setterAccessModifier.Value)));
        }

        return setAccessor;
    }

    private static SyntaxKind? GetSetterAccessModifier(TypeBuildingCommand request, ConfigureIdAttribute? attribute)
    {
        var modifier = request.Symbol.TypeKind switch
        {
            TypeKind.Struct => SyntaxKind.PrivateKeyword,
            TypeKind.Class => SyntaxKind.ProtectedKeyword,
            _ => throw new NotSupportedException("Only classes and structs are supported"),
        };

        return attribute?.SetterAccessibility.ToSyntaxKind(modifier) switch
        {
            SyntaxKind.PublicKeyword => null,
            var x => x,
        };
    }

    private static SyntaxKind GetSetterKind(ConfigureIdAttribute? attribute)
    {
        const SyntaxKind accessorKind = SyntaxKind.InitAccessorDeclaration;
        return attribute?.SetterType.ToSyntaxKind(accessorKind) ?? accessorKind;
    }
}
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

public class ParametrizedConstructorBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly ParameterSyntax Parameter;
    private static readonly BlockSyntax Body;

    static ParametrizedConstructorBuilder()
    {
        var left = IdentifierName("Id");
        var right = IdentifierName("id");
        var assignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);

        Parameter = Parameter(Identifier("id"));
        Body = Block(SingletonList<StatementSyntax>(ExpressionStatement(assignment)));
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var identifierSymbol = request.IdentifierSymbol;

        var hasParameterizedConstructor = request.Symbol.Constructors
            .Any(c => c.Parameters.Length is 1 && c.Parameters[0].Type.EqualsDefault(identifierSymbol));

        if (hasParameterizedConstructor)
            return next(request, context);

        var accessModifier = GetAccessModifier(request);

        var declaration = ConstructorDeclaration(Identifier(request.Symbol.Name))
            .AddModifiers(Token(accessModifier))
            .AddParameterListParameters(Parameter.WithType(IdentifierName(request.IdentifierSymbol.Name)))
            .WithBody(Body);

        request = request with
        {
            Root = request.Root.AddMembers(declaration)
        };

        return next(request, context);
    }

    private static SyntaxKind GetAccessModifier(TypeBuildingCommand request)
    {
        var accessModifier = request.Symbol.TypeKind is TypeKind.Struct
            ? SyntaxKind.PrivateKeyword
            : SyntaxKind.ProtectedKeyword;

        var attribute = request.Compilation
            .GetTypeByMetadataName(Constants.ConfigureConstructorsAttributeFullyQualifiedName);

        if (attribute is null || !request.Symbol.HasAttribute(attribute))
            return accessModifier;

        var syntax = request.Syntax.GetAttributeSyntax(attribute, request.Compilation);
        const string argumentName = nameof(ConfigureConstructorsAttribute.ParametrizedConstructorAccessibility);
        var argument = syntax.GetArgumentOrDefault(argumentName);
        var value = (argument?.Expression as MemberAccessExpressionSyntax)?.Name.ToString();

        return value switch
        {
            nameof(Accessibility.Private) => SyntaxKind.PrivateKeyword,
            nameof(Accessibility.Protected) => SyntaxKind.ProtectedKeyword,
            nameof(Accessibility.Internal) => SyntaxKind.InternalKeyword,
            nameof(Accessibility.Public) => SyntaxKind.PublicKeyword,
            _ => accessModifier
        };
    }
}
using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class GetHashCodeBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private const string MethodName = "GetHashCode";
    private static readonly TypeSyntax ReturnType = PredefinedType(Token(SyntaxKind.IntKeyword));
    private static readonly SyntaxToken MethodIdentifier = Identifier(MethodName);

    private static readonly SyntaxToken[] Modifiers =
    {
        Token(SyntaxKind.PublicKeyword),
        Token(SyntaxKind.OverrideKeyword)
    };

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        ArgumentSyntax[] arguments = request.Identifiers
            .Select(BuildIdentifierArgument)
            .ToArray();

        var tuple = TupleExpression().AddArguments(arguments);

        var memberAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            tuple,
            IdentifierName("GetHashCode"));

        var invocation = InvocationExpression(memberAccess);

        var declaration = MethodDeclaration(ReturnType, MethodIdentifier)
            .AddModifiers(Modifiers)
            .WithExpressionBody(ArrowExpressionClause(invocation))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        request = request with
        {
            Root = request.Root.AddMembers(declaration)
        };

        return next(request, context);
    }

    public ArgumentSyntax BuildIdentifierArgument(Identifier identifier)
        => Argument(IdentifierName(identifier.CapitalizedName));
}
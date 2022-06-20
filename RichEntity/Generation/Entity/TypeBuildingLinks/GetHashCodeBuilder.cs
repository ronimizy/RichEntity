using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class GetHashCodeBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private const string MethodName = "GetHashCode";
    private static readonly MethodDeclarationSyntax DeclarationSyntax;

    static GetHashCodeBuilder()
    {
        var returnType = PredefinedType(Token(SyntaxKind.IntKeyword));
        var methodIdentifier = Identifier(MethodName);
        var invocation = InvocationExpression(MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            IdentifierName("Id"),
            IdentifierName(MethodName)));

        DeclarationSyntax = MethodDeclaration(returnType, methodIdentifier)
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword))
            .WithExpressionBody(ArrowExpressionClause(invocation))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        request = request with
        {
            Root = request.Root.AddMembers(DeclarationSyntax)
        };

        return next(request, context);
    }
}
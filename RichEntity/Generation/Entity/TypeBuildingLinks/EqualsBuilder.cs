using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class EqualsBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly PredefinedTypeSyntax ReturnType = PredefinedType(Token(SyntaxKind.BoolKeyword));
    private static readonly SyntaxToken MethodIdentifier = Identifier("Equals");
    private static readonly ParameterSyntax Parameter = Parameter(Identifier("other"));
    private static readonly SyntaxToken DisableNullableTriviaBraceToken;
    private static readonly BinaryExpressionSyntax Expression;
    private static readonly SyntaxTokenList Modifiers;

    static EqualsBuilder()
    {
        var memberAccess = MemberAccessExpression
        (
            SyntaxKind.SimpleMemberAccessExpression,
            MemberBindingExpression(IdentifierName("Id")),
            IdentifierName("Equals")
        );

        var invocation = InvocationExpression(memberAccess).AddArgumentListArguments(Argument(IdentifierName("Id")));
        var left = ConditionalAccessExpression(IdentifierName("other"), invocation);
        var falseExpression = LiteralExpression(SyntaxKind.FalseLiteralExpression);

        var enableDirectiveTrivia = Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true));
        var restoreDirectiveTrivia = Trivia(NullableDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true));

        Expression = BinaryExpression(SyntaxKind.CoalesceExpression, left, falseExpression);
        Modifiers = TokenList(Token(TriviaList(enableDirectiveTrivia), SyntaxKind.PublicKeyword, TriviaList()));
        DisableNullableTriviaBraceToken = Token(TriviaList(restoreDirectiveTrivia), SyntaxKind.OpenBraceToken, TriviaList());
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var body = Block(SingletonList(ReturnStatement(Expression)))
            .WithOpenBraceToken(DisableNullableTriviaBraceToken);

        var declaration = MethodDeclaration(ReturnType, MethodIdentifier)
            .WithModifiers(Modifiers)
            .AddParameterListParameters(Parameter.WithType(NullableType(IdentifierName(request.Symbol.Name))))
            .WithBody(body);

        request = request with
        {
            Root = request.Root.AddMembers(declaration)
        };

        return next(request, context);
    }
}
using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class EqualsBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly PredefinedTypeSyntax ReturnType = PredefinedType(Token(SyntaxKind.BoolKeyword));
    private static readonly SyntaxToken MethodIdentifier = Identifier("Equals");
    private static readonly IdentifierNameSyntax MethodIdentifierName = IdentifierName("Equals");
    private static readonly ParameterSyntax Parameter = Parameter(Identifier("other"));
    private static readonly NameSyntax ParameterIdentifier = IdentifierName("other");
    private static readonly LiteralExpressionSyntax FalseLiteral = LiteralExpression(SyntaxKind.FalseLiteralExpression);
    
    private static readonly SyntaxToken DisableNullableTriviaBraceToken;
    private static readonly SyntaxTokenList Modifiers;

    static EqualsBuilder()
    {
        var enableDirectiveTrivia = Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true));
        var restoreDirectiveTrivia = Trivia(NullableDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true));

        Modifiers = TokenList(Token(TriviaList(enableDirectiveTrivia), SyntaxKind.PublicKeyword, TriviaList()));
        DisableNullableTriviaBraceToken = Token(
            TriviaList(restoreDirectiveTrivia), SyntaxKind.OpenBraceToken, TriviaList());
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var equalityCheck = request.Identifiers
            .Select(BuildIdentifierEqualityCheck)
            .Aggregate((a, b) => BinaryExpression(SyntaxKind.LogicalAndExpression, a, b));

        var body = Block(SingletonList(ReturnStatement(equalityCheck)))
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

    private static ExpressionSyntax BuildIdentifierEqualityCheck(Identifier identifier)
    {
        var memberAccess = MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            MemberBindingExpression(identifier.GetIdentifierName()),
            MethodIdentifierName);

        var invocation = InvocationExpression(memberAccess).AddArgumentListArguments(identifier.GetArgument());
        var conditionalAccess = ConditionalAccessExpression(ParameterIdentifier, invocation);

        return ParenthesizedExpression(BinaryExpression(
            SyntaxKind.CoalesceExpression, 
            conditionalAccess,
            FalseLiteral));
    }
}
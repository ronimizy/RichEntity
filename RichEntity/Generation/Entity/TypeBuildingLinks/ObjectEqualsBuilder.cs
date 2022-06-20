using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class ObjectEqualsBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly PredefinedTypeSyntax ReturnType = PredefinedType(Token(SyntaxKind.BoolKeyword));
    private static readonly SyntaxToken MethodIdentifier = Identifier("Equals");
    private static readonly IdentifierNameSyntax OtherIdentifierName = IdentifierName("other");
    private static readonly SyntaxToken DisableNullableTriviaBraceToken;
    private static readonly SyntaxTokenList Modifiers;
    private static readonly ParameterSyntax Parameter;
    private static readonly InvocationExpressionSyntax InvocationExpression;

    static ObjectEqualsBuilder()
    {
        var type = NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword)));
        var enableDirectiveTrivia = Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true));
        var restoreDirectiveTrivia = Trivia(NullableDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true));

        Modifiers = TokenList(
            Token(TriviaList(enableDirectiveTrivia), SyntaxKind.PublicKeyword, TriviaList()),
            Token(SyntaxKind.OverrideKeyword));
        
        DisableNullableTriviaBraceToken = Token(
            TriviaList(restoreDirectiveTrivia), SyntaxKind.OpenBraceToken, TriviaList());
        
        Parameter = Parameter(Identifier("other")).WithType(type);
        InvocationExpression = InvocationExpression(IdentifierName("Equals"));
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        if (request.Symbol.TypeKind is TypeKind.Struct)
            return next(request, context);

        var typeIdentifier = IdentifierName(request.Symbol.Name);
        var binaryExpression = BinaryExpression(SyntaxKind.AsExpression, OtherIdentifierName, typeIdentifier);
        var argument = Argument(binaryExpression);
        var expression = InvocationExpression.AddArgumentListArguments(argument);
        var body = Block(SingletonList(ReturnStatement(expression)))
            .WithOpenBraceToken(DisableNullableTriviaBraceToken);

        var declaration = MethodDeclaration(ReturnType, MethodIdentifier)
            .WithModifiers(Modifiers)
            .AddParameterListParameters(Parameter)
            .WithBody(body);

        request = request with
        {
            Root = request.Root.AddMembers(declaration)
        };

        return next(request, context);
    }
}
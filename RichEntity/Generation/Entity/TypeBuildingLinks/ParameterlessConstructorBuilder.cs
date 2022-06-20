using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class ParameterlessConstructorBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly SyntaxToken Modifiers;
    private static readonly SyntaxToken PragmaRestore;

    static ParameterlessConstructorBuilder()
    {
        var disableTrivia = PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true);
        var restoreTrivia = PragmaWarningDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true);
        var errorCode = IdentifierName("CS8618");
        
        var pragmaDisable = Trivia(disableTrivia.AddErrorCodes(errorCode));
        var pragmaRestore = Trivia(restoreTrivia.AddErrorCodes(errorCode));

        Modifiers = Token(TriviaList(pragmaDisable), SyntaxKind.ProtectedKeyword, TriviaList());
        PragmaRestore = Token(TriviaList(pragmaRestore), SyntaxKind.CloseBraceToken, TriviaList());
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        var needConstructor = !request.Symbol.Constructors
            .Any(c => !c.IsStatic && c.Parameters.Length == 0 && !c.IsImplicitlyDeclared);

        needConstructor = needConstructor && request.Symbol.TypeKind is not TypeKind.Struct;

        if (!needConstructor)
            return next(request, context);

        var declaration = ConstructorDeclaration(Identifier(request.Symbol.Name))
            .AddModifiers(Modifiers)
            .WithBody(Block().WithCloseBraceToken(PragmaRestore));

        request = request with
        {
            Root = request.Root.AddMembers(declaration)
        };

        return next(request, context);
    }
}
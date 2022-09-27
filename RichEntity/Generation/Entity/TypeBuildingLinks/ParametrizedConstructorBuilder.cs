using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Annotations;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Extensions;
using RichEntity.ObjectCreation;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = RichEntity.Annotations.Accessibility;
using Identifier = RichEntity.Generation.Entity.Models.Identifier;

namespace RichEntity.Generation.Entity.TypeBuildingLinks;

public class ParametrizedConstructorBuilder : ILink<TypeBuildingCommand, TypeDeclarationSyntax>
{
    private static readonly SyntaxTrivia PragmaDisable;
    private static readonly SyntaxToken PragmaRestoreToken;

    private readonly IChain<GetIdentifiersCommand, IEnumerable<Identifier>> _getIdentifiersChain;

    static ParametrizedConstructorBuilder()
    {
        var disableTrivia = PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true);
        var restoreTrivia = PragmaWarningDirectiveTrivia(Token(SyntaxKind.RestoreKeyword), true);
        var errorCode = IdentifierName("CS8618");
        var pragmaRestore = Trivia(restoreTrivia.AddErrorCodes(errorCode));

        PragmaRestoreToken = Token(TriviaList(pragmaRestore), SyntaxKind.OpenBraceToken, TriviaList());
        PragmaDisable = Trivia(disableTrivia.AddErrorCodes(errorCode));
    }

    public ParametrizedConstructorBuilder(IChain<GetIdentifiersCommand, IEnumerable<Identifier>> getIdentifiersChain)
    {
        _getIdentifiersChain = getIdentifiersChain;
    }

    public TypeDeclarationSyntax Process(
        TypeBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeBuildingCommand, SynchronousContext, TypeDeclarationSyntax> next)
    {
        if (HasParametrizedConstructor(request))
            return next(request, context);

        Identifier[] baseIdentifiers = _getIdentifiersChain
            .ProcessOrEmpty(request.Symbol.BaseType, request.Compilation)
            .ToArray();

        ParameterSyntax[] parameters = request.Identifiers
            .Select(i => i.GetParameter(false))
            .ToArray();

        StatementSyntax[] assignments = request.Identifiers
            .Except(baseIdentifiers)
            .Select(BuildIdentifierAssignmentStatement)
            .ToArray();

        (BlockSyntax body, SyntaxToken accessModifier) = GetAccessModifier(request) switch
        {
            var x and (SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword) =>
            (
                Block(assignments),
                Token(x)
            ),
            var x =>
            (
                Block(assignments).WithOpenBraceToken(PragmaRestoreToken),
                Token(TriviaList(PragmaDisable), x, TriviaList())
            ),
        };

        var declaration = ConstructorDeclaration(Identifier(request.Symbol.Name))
            .AddModifiers(accessModifier)
            .AddParameterListParameters(parameters)
            .WithBody(body);

        if (baseIdentifiers.Length is not 0)
        {
            ArgumentSyntax[] arguments = baseIdentifiers
                .Select(BuildIdentifierArgument)
                .ToArray();

            var initializer = ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                .AddArgumentListArguments(arguments);

            declaration = declaration.WithInitializer(initializer);
        }

        request = request with
        {
            Root = request.Root.AddMembers(declaration)
        };

        return next(request, context);
    }

    private static StatementSyntax BuildIdentifierAssignmentStatement(Identifier identifier)
    {
        var left = identifier.GetIdentifierName();
        var right = identifier.GetIdentifierName(false);

        return ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
    }

    private static ArgumentSyntax BuildIdentifierArgument(Identifier identifier)
    {
        return identifier.GetArgument(false)
            .WithNameColon(NameColon(IdentifierName(identifier.LowercasedName)));
    }

    private static bool HasParametrizedConstructor(TypeBuildingCommand request)
    {
        IReadOnlyList<Identifier> identifiers = request.Identifiers;

        bool NeededParameterizedConstructor(IMethodSymbol symbol)
            => !identifiers.Where((t, i) => !symbol.Parameters[i].Type.EqualsDefault(t.Type)).Any();

        return request.Symbol.Constructors
            .Where(c => c.Parameters.Length.Equals(request.Identifiers.Count))
            .Where(NeededParameterizedConstructor)
            .Any();
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
        var attributeObject = ObjectCreator.Create<ConfigureConstructorsAttribute>(syntax, request.Compilation);

        return attributeObject.ParametrizedConstructorAccessibility.ToSyntaxKind(accessModifier);
    }
}
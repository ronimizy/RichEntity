using FluentChaining;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Extensions;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.FileBuildingLinks;

public class ClassBuilder : ILink<FileBuildingCommand, CompilationUnitSyntax>
{
    private readonly IChain<TypeBuildingCommand, TypeDeclarationSyntax> _classBuildingChain;

    public ClassBuilder(IChain<TypeBuildingCommand, TypeDeclarationSyntax> classBuildingChain)
    {
        _classBuildingChain = classBuildingChain;
    }

    public CompilationUnitSyntax Process(
        FileBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<FileBuildingCommand, SynchronousContext, CompilationUnitSyntax> next)
    {
        var modifiers = TokenList
        (
            request.Symbol.DeclaredAccessibility
                .ToSyntaxTokenList()
                .Append(Token(SyntaxKind.PartialKeyword))
        );

        var namespaceIdentifier = IdentifierName(request.Symbol.ContainingNamespace.GetFullyQualifiedName());
        var namespaceDeclaration = NamespaceDeclaration(namespaceIdentifier);
        var declaration = request.Symbol.ToSyntax().WithModifiers(modifiers);
        var command = new TypeBuildingCommand
        (
            request.Syntax,
            request.Symbol,
            request.IdentifierSymbol,
            declaration,
            request.Context.Compilation
        );

        declaration = _classBuildingChain.Process(command);
        namespaceDeclaration = namespaceDeclaration.AddMembers(declaration);

        request = request with
        {
            Root = request.Root.AddMembers(namespaceDeclaration)
        };

        return next(request, context);
    }
}
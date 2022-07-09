using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using RichEntity.Generation.Entity.Models;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.SourceGenerators;

[Generator]
public abstract class EntitySourceGeneratorBase : GeneratorBase<FileBuildingCommand>
{
    protected sealed override IChain<FileBuildingCommand, CompilationUnitSyntax> Chain { get; init; }

    protected override bool TryBuildRequest(GeneratorExecutionContext context, SyntaxNode node, out FileBuildingCommand request)
    {
        request = default;
        
        if (node is not TypeDeclarationSyntax syntax)
            return false;
        
        var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

        if (semanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol entitySymbol)
            return false;

        IReadOnlyCollection<Identifier> identifiers = GetIdentifiers(context, entitySymbol, semanticModel);

        request = new FileBuildingCommand(
            syntax,
            entitySymbol,
            identifiers,
            CompilationUnit(),
            context);

        return true;
    }

    protected override string GetFileName(GeneratorExecutionContext context, SyntaxNode node, FileBuildingCommand request)
    {
        if (node is not TypeDeclarationSyntax syntax)
            return string.Empty;
        
        return $"{syntax.Identifier}.{Constants.FilenameSuffix}";
    }

    protected abstract IReadOnlyCollection<Identifier> GetIdentifiers(
        GeneratorExecutionContext context,
        INamedTypeSymbol entitySymbol,
        SemanticModel semanticModel);
}
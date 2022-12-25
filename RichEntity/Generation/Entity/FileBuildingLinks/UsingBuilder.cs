using FluentChaining;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.FileBuildingLinks;

public class UsingBuilder : ILink<FileBuildingCommand, CompilationUnitSyntax>
{
    private readonly IChain<CommentHeaderBuildingCommand, UsingDirectiveSyntax> _commentChain;

    public UsingBuilder(IChain<CommentHeaderBuildingCommand, UsingDirectiveSyntax> commentChain)
    {
        _commentChain = commentChain;
    }

    public CompilationUnitSyntax Process(
        FileBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<FileBuildingCommand, SynchronousContext, CompilationUnitSyntax> next)
    {
        var directive = UsingDirective(IdentifierName("System"));
        var commentBuildingCommand = new CommentHeaderBuildingCommand(directive);

        directive = _commentChain.Process(commentBuildingCommand);

        var syntax = request.Root.AddUsings(directive);

        request = request with
        {
            Root = syntax,
        };

        return next(request, context);
    }
}
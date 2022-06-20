using FluentChaining;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Generation.Entity.FileBuildingLinks;

public class UsingBuilder : ILink<FileBuildingCommand, CompilationUnitSyntax>
{
    public CompilationUnitSyntax Process(
        FileBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<FileBuildingCommand, SynchronousContext, CompilationUnitSyntax> next)
    {
        var syntax = request.Root
            .AddUsings(UsingDirective(IdentifierName("System")));

        request = request with
        {
            Root = syntax
        };
        
        return next(request, context);
    }
}
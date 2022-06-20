using FluentChaining;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Commands;

namespace RichEntity.Generation.Entity.FileBuildingLinks;

public class TypePreconditionFilter : ILink<FileBuildingCommand, CompilationUnitSyntax>
{
    private readonly IChain<TypeCheckingCommand, bool> _typeCheckingChain;

    public TypePreconditionFilter(IChain<TypeCheckingCommand, bool> typeCheckingChain)
    {
        _typeCheckingChain = typeCheckingChain;
    }

    public CompilationUnitSyntax Process(
        FileBuildingCommand request,
        SynchronousContext context,
        LinkDelegate<FileBuildingCommand, SynchronousContext, CompilationUnitSyntax> next)
    {
        var typeCheckingCommand = new TypeCheckingCommand
        (
            request.Syntax,
            request.Symbol,
            request.IdentifierSymbol,
            request.Context
        );
        
        var typeCheckingResult = _typeCheckingChain.Process(typeCheckingCommand);
        return typeCheckingResult ? next(request, context) : request.Root;
    }
}
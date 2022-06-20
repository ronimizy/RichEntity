using FluentChaining;
using RichEntity.Analysis.Entity;
using RichEntity.Generation.Entity.Commands;

namespace RichEntity.Generation.Entity.TypePreconditionLinks;

public class PartialTypeChecker : ILink<TypeCheckingCommand, bool>
{
    public bool Process(
        TypeCheckingCommand request,
        SynchronousContext context,
        LinkDelegate<TypeCheckingCommand, SynchronousContext, bool> next)
    {
        var conforming = PartialTypeAnalyzer.Conforming(request.Syntax);
        return next(request, context) && conforming;
    }
}
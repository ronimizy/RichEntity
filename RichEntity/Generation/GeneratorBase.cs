using FluentChaining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Generation;

public abstract class GeneratorBase<TRequest> : ISourceGenerator
{
    protected abstract IChain<TRequest, CompilationUnitSyntax> Chain { get; init;  }

    public abstract void Initialize(GeneratorInitializationContext context);

    public void Execute(GeneratorExecutionContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        IReadOnlyCollection<SyntaxNode>? nodes = GetNodes(context);
        
        if (nodes is null)
            return;

        foreach (var node in nodes)
        {
            ProcessNode(context, node);
        }
    }

    protected abstract IReadOnlyCollection<SyntaxNode>? GetNodes(GeneratorExecutionContext context);

    protected abstract bool TryBuildRequest(GeneratorExecutionContext context, SyntaxNode node, out TRequest request);

    protected abstract string GetFileName(GeneratorExecutionContext context, SyntaxNode node, TRequest request);

    private void ProcessNode(GeneratorExecutionContext context, SyntaxNode node)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        if (!TryBuildRequest(context, node, out var request))
            return;

        var compilationUnit = Chain.Process(request).NormalizeWhitespace();
        var fileName = GetFileName(context, node, request);

        context.AddSource(fileName, compilationUnit.ToString());
    }
}
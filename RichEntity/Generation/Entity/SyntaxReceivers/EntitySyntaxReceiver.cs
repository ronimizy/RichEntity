using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Generation.Entity.SyntaxReceivers;

public class EntitySyntaxReceiver : ISyntaxReceiver
{
    private readonly List<TypeDeclarationSyntax> _nodes = new List<TypeDeclarationSyntax>();

    public IReadOnlyCollection<TypeDeclarationSyntax> Nodes => _nodes;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TypeDeclarationSyntax { BaseList.Types.Count: > 0 } declarationSyntax)
            return;

        _nodes.Add(declarationSyntax);
    }
}
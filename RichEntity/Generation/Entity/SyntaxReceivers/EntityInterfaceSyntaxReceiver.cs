using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Utility;

namespace RichEntity.Generation.Entity.SyntaxReceivers;

public class EntityInterfaceSyntaxReceiver : ISyntaxReceiver
{
    private readonly List<TypeDeclarationSyntax> _nodes = new List<TypeDeclarationSyntax>();

    public IReadOnlyCollection<TypeDeclarationSyntax> Nodes => _nodes;

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not TypeDeclarationSyntax { BaseList: not null } declarationSyntax)
            return;

        IEnumerable<GenericNameSyntax> interfaces = declarationSyntax.BaseList.Types
            .Select(t => t.Type)
            .OfType<GenericNameSyntax>()
            .Where(t => Constants.EntityInterfaceName.Equals(t.Identifier.ToString()));

        if (interfaces.Any())
            _nodes.Add(declarationSyntax);
    }
}
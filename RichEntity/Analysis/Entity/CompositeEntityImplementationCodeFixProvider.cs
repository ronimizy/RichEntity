using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Utility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Analysis.Entity;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CompositeEntityImplementationCodeFixProvider))]
public class CompositeEntityImplementationCodeFixProvider : CodeFixProvider
{
    public const string Title = "Implement IEntity interface";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(CompositeEntityImplementationAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

        if (diagnostic is null)
            return;

        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root?.FindNode(context.Span) is not TypeDeclarationSyntax syntax)
            return;

        var codeAction = CodeAction.Create
        (
            Title,
            c => FixDeclaration(context.Document, syntax, c),
            nameof(Title)
        );

        context.RegisterCodeFix(codeAction, diagnostic);
    }

    private static async Task<Document> FixDeclaration(
        Document document,
        TypeDeclarationSyntax syntax,
        CancellationToken cancellationToken)
    {
        var fixedSyntax = syntax.AddBaseListTypes(SimpleBaseType(IdentifierName(Constants.EntityInterfaceName)));

        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = oldRoot!.ReplaceNode(syntax, fixedSyntax);

        var unit = newRoot.DescendantNodesAndSelf().OfType<CompilationUnitSyntax>().SingleOrDefault();

        if (unit is not null && !unit.Usings.Any(u => u.Name.ToString().Equals(Constants.AnnotationsNamespace)))
        {
            var usingSyntax = UsingDirective(IdentifierName(Constants.AnnotationsNamespace));
            var fixedUnit = unit.AddUsings(usingSyntax);

            newRoot = newRoot.ReplaceNode(unit, fixedUnit);
        }

        return document.WithSyntaxRoot(newRoot.NormalizeWhitespace());
    }
}
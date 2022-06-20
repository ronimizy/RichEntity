using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Analysis.Entity;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PartialTypeCodeFixProvider))]
public class PartialTypeCodeFixProvider : CodeFixProvider
{
    public const string Title = "Make type partial";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(PartialTypeAnalyzer.DiagnosticId);

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
        var newSyntax = syntax.AddModifiers(Token(SyntaxKind.PartialKeyword)).NormalizeWhitespace();

        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var newRoot = oldRoot!.ReplaceNode(syntax, newSyntax);

        return document.WithSyntaxRoot(newRoot);
    }
}
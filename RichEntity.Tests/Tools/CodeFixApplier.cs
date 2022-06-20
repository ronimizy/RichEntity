using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace RichEntity.Analyzers.Tests.Tools;

public static class CodeFixApplier
{
    public static async Task<SourceText> GetUpdatedSourceTextAsync(
        string code,
        IReadOnlyCollection<Type> referencedTypes,
        DiagnosticAnalyzer analyzer,
        params CodeFixProvider[] providers)
    {
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable);

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp9);

        var source = new SourceFile("File.cs", code);

        var project = CompilationBuilder.BuildProject(referencedTypes, compilationOptions, parseOptions, source);
        var compilation = (await project.GetCompilationAsync())!;
        var compilationWithAnalyzer = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

        var diagnostics = await compilationWithAnalyzer.GetAllDiagnosticsAsync();

        var codeActions = new List<CodeAction>();
        foreach (var diagnostic in diagnostics)
        {
            var context = new CodeFixContext(project.Documents.Single(), diagnostic,
                (action, _) => { codeActions.Add(action); }, CancellationToken.None);

            foreach (var provider in providers)
            {
                await provider.RegisterCodeFixesAsync(context);
            }
        }

        var operationTasks = codeActions.Select(c => c.GetOperationsAsync(CancellationToken.None));
        var operations = (await Task.WhenAll(operationTasks)).SelectMany(o => o);

        var workspace = project.Solution.Workspace;
        foreach (var operation in operations)
        {
            operation.Apply(workspace, CancellationToken.None);
        }

        return await workspace.CurrentSolution.Projects
            .Single()
            .Documents
            .Single()
            .GetTextAsync();
    }
}
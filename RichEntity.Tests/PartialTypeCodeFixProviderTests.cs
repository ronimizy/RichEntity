using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using RichEntity.Analysis.Entity;
using RichEntity.Analyzers.Tests.Tools;

namespace RichEntity.Analyzers.Tests;

[TestFixture]
public class PartialTypeCodeFixProviderTests
{
    private DiagnosticAnalyzer _partialTypeAnalyzer = null!;
    private CodeFixProvider _partialTypeCodeFixProvider = null!;
    
    [SetUp]
    public void SetUp()
    {
        _partialTypeAnalyzer = new PartialTypeAnalyzer();
        _partialTypeCodeFixProvider = new PartialTypeCodeFixProvider();
    }
    
    [Test]
    public async Task Test()
    {
        var code = await File.ReadAllTextAsync(@"Generation/Entity/Sample.cs");

        var updateSource = await GetUpdatedSourceTextAsync(code, _partialTypeAnalyzer, _partialTypeCodeFixProvider);
        Console.WriteLine(updateSource);

        Assert.Pass();
    }

    private static async Task<SourceText> GetUpdatedSourceTextAsync(
        string code,
        DiagnosticAnalyzer analyzer,
        params CodeFixProvider[] providers)
    {
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication,
            nullableContextOptions: NullableContextOptions.Enable);

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp10);

        Type[] references =
        {
            typeof(object),
            typeof(Annotations.IEntity<>),
            typeof(Console)
        };

        var project = CompilationBuilder.BuildProject
        (
            references,
            compilationOptions,
            parseOptions,
            new SourceFile("Source.cs", code)
        );

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

        return await workspace.CurrentSolution.Projects.Single().Documents.Single().GetTextAsync();
    }
}
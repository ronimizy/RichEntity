using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using RichEntity.Analysis.Entity;
using RichEntity.Analyzers.Tests.Tools;
using RichEntity.Generation.Entity;
using RichEntity.Utility;

namespace RichEntity.Analyzers.Tests;

[TestFixture]
public class EntityGeneratorTests
{
    [Test]
    public async Task SimpleGeneratorTest()
    {
        var programSource = new SourceFile("Sample.cs", await File.ReadAllTextAsync(@"Generation/Entity/Sample.cs"));

        Type[] referencedTypes =
        {
            typeof(object),
            typeof(Annotations.IEntity<>),
            typeof(Console)
        };

        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
        var compilation = await CompilationBuilder.BuildCompilation
        (
            referencedTypes,
            compilationOptions: options,
            sourceFiles: programSource
        );

        var newComp = RunGenerators(
            compilation,
            out ImmutableArray<Diagnostic> diagnostics,
            new EntitySourceGenerator());

        var analyzedComp = newComp.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new PartialTypeAnalyzer()));

        IEnumerable<SyntaxTree> newFiles = newComp.SyntaxTrees
            .Where(t => Path.GetFileName(t.FilePath).EndsWith(Constants.FilenameSuffix));

        diagnostics = diagnostics.Concat(await analyzedComp.GetAllDiagnosticsAsync()).ToImmutableArray();

        foreach (var file in newFiles)
        {
            Console.WriteLine(file);
            Console.WriteLine(new string('-', 30));
        }

        foreach (var diagnostic in diagnostics.Where(d => d.Severity is DiagnosticSeverity.Error))
        {
            Console.WriteLine(diagnostic);
        }

        Assert.False(diagnostics.Any(d => d.Severity is DiagnosticSeverity.Error));
    }

    private static Compilation RunGenerators(
        Compilation compilation,
        out ImmutableArray<Diagnostic> diagnostics,
        params ISourceGenerator[] generators)
    {
        CSharpGeneratorDriver
            .Create(generators)
            .RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics);

        return newCompilation;
    }
}
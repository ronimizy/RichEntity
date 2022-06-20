using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RichEntity.Analyzers.Tests.Tools;
using RichEntity.EntityFrameworkCore.Analysis.StringLiteral;

namespace RichEntity.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public class RE1000Tests
{
    [Test]
    public async Task ProperEntityConfigurationGeneratesNoDiagnostic()
    {
        var code = await File.ReadAllTextAsync(@"Proper.cs");

        var diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));

        foreach (var diagnostic in diagnostics)
        {
            Console.WriteLine(diagnostic);
        }

        Assert.AreEqual(0, diagnostics.Count);
    }

    [Test]
    public async Task InvalidEntityConfigurationGeneratesDiagnostics()
    {
        const int firstStart = 48;
        const int firstEnd = 69;

        const int secondStart = 81;
        const int secondEnd = 88;

        var code = await File.ReadAllTextAsync(@"Invalid.cs");

        var diagnostics = (await GetDiagnostics(code, typeof(object), typeof(ModelBuilder))).ToList();

        for (int i = firstStart; i <= firstEnd; i++)
        {
            Assert.IsTrue(
                diagnostics.Any(d => CheckDiagnosticAt(d, i)),
                $"Failed at line {firstStart + 1}");
        }
    }

    private static async Task<List<Diagnostic>> GetDiagnostics(string code, params Type[] referencesTypes)
    {
        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Annotations);

        var compilation = await CompilationBuilder.BuildCompilation(
            referencesTypes,
            compilationOptions: options,
            sourceFiles: new SourceFile("File", code));

        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidStringLiteralMemberNameDeclarationAnalyzer()));

        return (await compilationWithAnalyzers.GetAllDiagnosticsAsync()).ToList();
    }

    private static bool CheckDiagnosticAt(Diagnostic diagnostic, int line)
        => diagnostic.Location.GetLineSpan().StartLinePosition.Line == line - 1 &&
           diagnostic.Id.Equals(InvalidStringLiteralMemberNameDeclarationAnalyzer.Id);
}
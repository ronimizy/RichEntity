using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RichEntity.Analyzers.Suppressors;
using RichEntity.Analyzers.Tests.Tools;

namespace RichEntity.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public class RE2000Tests
{
    [Test]
    public async Task NullableDiagnosticSuppressed()
    {
        var code = await File.ReadAllTextAsync(@"Invalid.cs");

        List<Diagnostic> diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));

        Assert.IsTrue(diagnostics.Any(d => d is { Id: "CS8618", IsSuppressed: true }));
    }

    private static async Task<List<Diagnostic>> GetDiagnostics(string code, params Type[] referencesTypes)
    {
        var compilation = await CompilationBuilder.Build(code, referencesTypes);
        var compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(new UninitializedDbSetDiagnosticSuppressor()));
        return (await compilationWithAnalyzers.GetAllDiagnosticsAsync()).ToList();
    }
}
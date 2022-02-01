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
using RichEntity.Analyzers.Analyzers;
using RichEntity.Analyzers.Tests.Tools;

namespace RichEntity.Analyzers.Tests
{
    // ReSharper disable once InconsistentNaming
    public class RE1000Tests
    {
        [Test]
        public async Task ProperEntityConfigurationGeneratesNoDiagnostic()
        {
            var code = await File.ReadAllTextAsync(@"Proper.cs");

            List<Diagnostic> diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));

            Assert.AreEqual(0, diagnostics.Count);
        }

        [Test]
        public async Task InvalidEntityConfigurationGeneratesDiagnostics()
        {
            var code = await File.ReadAllTextAsync(@"Invalid.cs");

            var diagnostics = (await GetDiagnostics(code, typeof(object), typeof(ModelBuilder))).ToList();

            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 46)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 47)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 48)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 49)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 50)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 51)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 52)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 53)));
            
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 66)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 67)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 68)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 69)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 70)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 71)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 72)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 73)));
        }

        private static async Task<List<Diagnostic>> GetDiagnostics(string code, params Type[] referencesTypes)
        {
            var compilation = await CompilationBuilder.Build(code, referencesTypes);
            var compilationWithAnalyzers =
                compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidStringLiteralMemberNameDeclarationAnalyzer()));
            return (await compilationWithAnalyzers.GetAllDiagnosticsAsync()).ToList();
        }

        private static bool CheckDiagnosticAt(Diagnostic diagnostic, int line)
            => diagnostic.Location.GetLineSpan().StartLinePosition.Line == line - 1 &&
               diagnostic.Id.Equals(InvalidStringLiteralMemberNameDeclarationAnalyzer.Id);
    }
}
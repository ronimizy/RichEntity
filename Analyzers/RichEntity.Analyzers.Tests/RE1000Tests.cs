using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using RichEntity.Analyzers.Analyzers;
using RichEntity.Analyzers.Sample.Types;
using CompilationBuilder = RichEntity.Core.Utility.CompilationBuilder;

namespace RichEntity.Analyzers.Tests
{
    // ReSharper disable once InconsistentNaming
    public class RE1000Tests
    {
        [Test]
        public async Task ProperEntityConfigurationGeneratesNoDiagnostic()
        {
            var code = await File.ReadAllTextAsync(@"Proper.cs");

            List<Diagnostic> diagnostics =
                await GetDiagnostics(code, typeof(object), typeof(ModelBuilder), typeof(Invalid.Person));
            PrintDiagnostics(diagnostics);

            Assert.AreEqual(0, diagnostics.Count);
        }

        [Test]
        public async Task InvalidEntityConfigurationGeneratesDiagnostics()
        {
            const int firstStart = 48;
            const int firstEnd = 69;

            var code = await File.ReadAllTextAsync(@"Invalid.cs");

            var diagnostics = (await GetDiagnostics(
                    code,
                    typeof(object), typeof(ModelBuilder), typeof(CompletionProvider), typeof(Invalid.Person)))
                .ToList();

            PrintDiagnostics(diagnostics);

            for (int i = firstStart; i <= firstEnd; i++)
            {
                Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, i)), $"Failed at line {i}");
            }
        }

        private static async Task<List<Diagnostic>> GetDiagnostics(string code, params Type[] referencesTypes)
        {
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable,
                metadataImportOptions: MetadataImportOptions.Public);

            var compilation = await CompilationBuilder.Build(code, options, referencesTypes);
            var compilationWithAnalyzers =
                compilation.WithAnalyzers(
                    ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidStringLiteralMemberNameDeclarationAnalyzer()));
            return (await compilationWithAnalyzers.GetAllDiagnosticsAsync()).ToList();
        }

        private static bool CheckDiagnosticAt(Diagnostic diagnostic, int line)
            => diagnostic.Location.GetLineSpan().StartLinePosition.Line == line - 1 &&
               diagnostic.Id.Equals(InvalidStringLiteralMemberNameDeclarationAnalyzer.Id);

        private static void PrintDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics)
            {
                Console.WriteLine(diagnostic);
            }
        }
    }
}
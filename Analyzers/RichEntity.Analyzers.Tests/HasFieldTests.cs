using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class HasFieldTests
    {
        [Test]
        public async Task EmptyMethodGeneratesNoDiagnostics()
        {
            var code = @"
                public class Program
                {
                    public static void Main(string[] args) { }
                }";

            List<Diagnostic> diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));

            Assert.AreEqual(0, diagnostics.Count);
        }

        [Test]
        public async Task ProperEntityConfigurationGeneratesNoDiagnostic()
        {
            var code = @"
                using System.Collections.Generic;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                public class Program
                {
                    public static void Main(string[] args) { }
                }

                public class B
                {
                    public class Class
                    {
                        private readonly IReadOnlyCollection<int> _a = new List<int>();

                        public IReadOnlyCollection<int> A => _a;
                    }
                }

                internal class DriverConfiguration : IEntityTypeConfiguration<B.Class>
                {
                    private const string N = ""_a"";
                    public void Configure(EntityTypeBuilder<B.Class> builder)
                    {
                        builder.Navigation(d => d.A).HasField(""_a"");
                        builder.Navigation(d => d.A).HasField(N);
                    }
                }

                public sealed class DatabaseContext : DbContext
                {
                    private const string N = ""_a"";
                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {   
                        modelBuilder.Entity<B.Class>().Navigation(m => m.A).HasField(""_a"");
                        modelBuilder.Entity<B.Class>().Navigation(m => m.A).HasField(N);
                    }
                }";

            List<Diagnostic> diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));

            Assert.AreEqual(0, diagnostics.Count);
        }

        [Test]
        public async Task InvalidEntityConfigurationGeneratesDiagnostics()
        {
            var code = @"
                using System.Collections.Generic;
                using Microsoft.EntityFrameworkCore;
                using Microsoft.EntityFrameworkCore.Metadata.Builders;

                public class Program
                {
                    public static void Main(string[] args) { }
                }

                public class B
                {
                    public class Class
                    {
                        private readonly IReadOnlyCollection<int> _a = new List<int>();

                        public IReadOnlyCollection<int> A => _a;
                    }
                }

                internal class DriverConfiguration : IEntityTypeConfiguration<B.Class>
                {
                    private const string N = ""a"";

                    public void Configure(EntityTypeBuilder<B.Class> builder)
                    {
                        builder.Navigation(d => d.A).HasField(""a"");
                        builder.Navigation(d => d.A).HasField(N);
                    }
                }

                public sealed class DatabaseContext : DbContext
                {
                    private const string N = ""a"";

                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {   
                        modelBuilder.Entity<B.Class>().Navigation(m => m.A).HasField(N);
                        modelBuilder.Entity<B.Class>().Navigation(m => m.A).HasField(""a"");
                    }
                }";

            var diagnostics = (await GetDiagnostics(code, typeof(object), typeof(ModelBuilder))).ToList();

            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 26)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 27)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 37)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 38)));
        }

        private static async Task<List<Diagnostic>> GetDiagnostics(string code, params Type[] referencesTypes)
        {
            var compilation = await CompilationBuilder.Build(code, referencesTypes);
            var compilationWithAnalyzers =
                compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidStringLiteralMemberNameDeclarationAnalyzer()));
            return (await compilationWithAnalyzers.GetAllDiagnosticsAsync()).ToList();
        }

        private static bool CheckDiagnosticAt(Diagnostic diagnostic, int line)
            => diagnostic.Location.GetLineSpan().StartLinePosition.Line == line &&
               diagnostic.Id.Equals(InvalidStringLiteralMemberNameDeclarationAnalyzer.Id);
    }
}
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
    public class PropertyTests
    {
        [Test]
        public async Task EmptyMethodGeneratesNoDiagnostics()
        {
            var code = @"
                public class Program
                {
                    public static void Main(string[] args) { }
                }";

            var diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));
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
                        public IReadOnlyCollection<int> A { get; } = new List<int>();
                    }
                }

                internal class DriverConfiguration : IEntityTypeConfiguration<B.Class>
                {
                    public void Configure(EntityTypeBuilder<B.Class> builder)
                    {
                        builder.Property(""A"");
                        builder.Property(typeof(List<int>), ""A"");
                    }
                }

                public sealed class DatabaseContext : DbContext
                {
                    private const string N = ""A"";
                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {   
                        modelBuilder.Entity<B.Class>().Property(""A"");
                        modelBuilder.Entity<B.Class>().Property(typeof(List<int>), ""A"");
                        modelBuilder.Entity<B.Class>().Property(N);
                        modelBuilder.Entity<B.Class>().Property(typeof(List<int>), N);
                    }
                }";
            
            var diagnostics = await GetDiagnostics(code, typeof(object), typeof(ModelBuilder));
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
                        public IReadOnlyCollection<int> B { get; } = new List<int>();
                    }
                }

                internal class DriverConfiguration : IEntityTypeConfiguration<B.Class>
                {
                    public void Configure(EntityTypeBuilder<B.Class> builder)
                    {
                        builder.Property(""A"");
                        builder.Property(typeof(List<int>), ""A"");
                    }
                }

                public sealed class DatabaseContext : DbContext
                {
                    private const string N = ""A"";
                    protected override void OnModelCreating(ModelBuilder modelBuilder)
                    {   
                        modelBuilder.Entity<B.Class>().Property(""A"");
                        modelBuilder.Entity<B.Class>().Property(typeof(List<int>), ""A"");
                        modelBuilder.Entity<B.Class>().Property(N);
                        modelBuilder.Entity<B.Class>().Property(typeof(List<int>), N);
                    }
                }";

            var diagnostics = (await GetDiagnostics(code, typeof(object), typeof(ModelBuilder))).ToList();

            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 22)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 23)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 32)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 33)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 34)));
            Assert.IsTrue(diagnostics.Any(d => CheckDiagnosticAt(d, 35)));
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
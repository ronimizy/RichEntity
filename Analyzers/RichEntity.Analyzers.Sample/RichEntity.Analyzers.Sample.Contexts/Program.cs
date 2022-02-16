using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using RichEntity.Analyzers.Analyzers;
using RichEntity.Core.Utility;

namespace RichEntity.Analyzers.Sample;

public class Program
{
    private const string Code = @"
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace RichEntity.Analyzers.Sample.Contexts;

public class Invalid
{
    public class Person
    {
        private readonly List<Person> _children;
        private readonly Person? _father;
        private readonly string _name;

        public Person(string name, Person? father)
        {
            _children = new List<Person>();
            _name = name;
            _father = father;
        }

        protected Person() { }

        public string Name => _name;
        public Person? Father => _father;

        public IReadOnlyCollection<Person> Children => _children;

        public void AddChild(Person person)
            => _children.Add(person);
    }

    public class Context : DbContext
    {
        private const string Children = ""_childrens"";
        private const string Name = ""name"";
        private const string Father = ""father"";
        private const string Friends = ""_friends"";

        public DbSet<Person> Persons { get; private set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var eb = modelBuilder.Entity<Person>();
            var nb = eb.Navigation(p => p.Children);

            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField(""_childrens"");
            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField(Children);
            modelBuilder.Entity<Person>().Property(""name"");
            modelBuilder.Entity<Person>().Property(Name);
            modelBuilder.Entity<Person>().HasOne(""father"");
            modelBuilder.Entity<Person>().HasOne(Father);
            modelBuilder.Entity<Person>().HasMany(""friends"");
            modelBuilder.Entity<Person>().HasMany(Friends);
            modelBuilder.Entity<Person>().OwnsMany(""adad"", ""adaadd"");
            modelBuilder.Entity<Person>().OwnsOne<Person>(""a"", ""b"");
            eb.Navigation(p => p.Children).HasField(""_childrens"");
            eb.Navigation(p => p.Children).HasField(Children);
            eb.Property(""name"");
            eb.Property(Name);
            eb.HasOne(""father"");
            eb.HasOne(Father);
            eb.HasMany(""friends"");
            eb.HasMany(Friends);
            eb.OwnsMany(""adad"", ""adadad"");
            eb.OwnsOne(""aa"", ""ada"");
            nb.HasField(""_childrens"");
            nb.HasField(Children);
        }
    }
}";
    
    public static async Task Main(string[] args)
    {
        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableContextOptions.Enable,
            metadataImportOptions: MetadataImportOptions.Public);
        var _ = typeof(Microsoft.CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions);

        var compilation = await CompilationBuilder.Build(Code, options, typeof(object), typeof(ModelBuilder));
        var compilationWithAnalyzers =
            compilation.WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(new InvalidStringLiteralMemberNameDeclarationAnalyzer()));
        var a = (await compilationWithAnalyzers.GetAllDiagnosticsAsync()).ToList();
        Console.WriteLine(a);
    }
}
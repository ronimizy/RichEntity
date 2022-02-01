using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS8618

namespace RichEntity.Analyzers.Sample;

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
        private const string Children = "_childrens";
        private const string Name = "name";
        private const string Father = "father";
        private const string Friends = "_friends";

        public DbSet<Person> Persons { get; private set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField("_childrens");
            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField(Children);
            modelBuilder.Entity<Person>().Property("name");
            modelBuilder.Entity<Person>().Property(Name);
            modelBuilder.Entity<Person>().HasOne("father");
            modelBuilder.Entity<Person>().HasOne(Father);
            modelBuilder.Entity<Person>().HasMany("friends");
            modelBuilder.Entity<Person>().HasMany(Friends);
        }
    }

    public class Configuration : IEntityTypeConfiguration<Person>
    {
        private const string Children = "_childrens";
        private const string Name = "name";
        private const string Father = "father";
        private const string Friends = "_friends";

        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.Navigation(p => p.Children).HasField("_childrens");
            builder.Navigation(p => p.Children).HasField(Children);
            builder.Property("name");
            builder.Property(Name);
            builder.HasOne("father");
            builder.HasOne(Father);
            builder.HasMany("_friends");
            builder.HasMany(Friends);
        }
    }
}
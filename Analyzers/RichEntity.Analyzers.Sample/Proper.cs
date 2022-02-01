using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

#pragma warning disable CS8618

namespace RichEntity.Analyzers.Playground;

public class Proper
{
    public class Person
    {
        private readonly List<Person> _friends;
        private readonly List<Person> _children;
        private readonly Person? _father;
        private readonly string _name;

        public Person(string name, Person? father)
        {
            _children = new List<Person>();
            _friends = new List<Person>();
            _name = name;
            _father = father;
        }

        protected Person() { }

        public string Name => _name;
        public Person? Father => _father;

        public IReadOnlyCollection<Person> Children => _children;

        public void AddChild(Person person)
            => _children.Add(person);

        public void AddFriend(Person person)
            => _friends.Add(person);
    }

    public class Context : DbContext
    {
        public DbSet<Person> Persons { get; private set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField("_children");
            modelBuilder.Entity<Person>().Property("_name");
            modelBuilder.Entity<Person>().HasOne("_father");
            modelBuilder.Entity<Person>().HasMany("_friends");
        }
    }

    public class Configuration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.Navigation(p => p.Children).HasField("_children");
            builder.Property("_name");
            builder.HasOne("_father");
            builder.HasMany("_friends");
        }
    }
}
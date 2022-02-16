using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


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

#pragma warning disable CS8618
        protected Person() { }
#pragma warning restore CS8618

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

        public DbSet<Person> Persons { get; set; }

        public Context(DbContextOptions<Context> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var eb = modelBuilder.Entity<Person>();
            var nb = eb.Navigation(p => p.Children);

            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField("_childrens");
            modelBuilder.Entity<Person>().Navigation(p => p.Children).HasField(Children);
            modelBuilder.Entity<Person>().Property("name");
            modelBuilder.Entity<Person>().Property(Name);
            modelBuilder.Entity<Person>().HasOne("father");
            modelBuilder.Entity<Person>().HasOne(Father);
            modelBuilder.Entity<Person>().HasMany("friends");
            modelBuilder.Entity<Person>().HasMany(Friends);
            modelBuilder.Entity<Person>().OwnsMany("adad", "adaadd");
            modelBuilder.Entity<Person>().OwnsOne<Person>("a", "b");
            eb.Navigation(p => p.Children).HasField("_childrens");
            eb.Navigation(p => p.Children).HasField(Children);
            eb.Property("name");
            eb.Property(Name);
            eb.HasOne("father");
            eb.HasOne(Father);
            eb.HasMany("friends");
            eb.HasMany(Friends);
            eb.OwnsMany("adad", "adadad");
            eb.OwnsOne("aa", "ada");
            nb.HasField("_childrens");
            nb.HasField(Children);
        }
    }
}
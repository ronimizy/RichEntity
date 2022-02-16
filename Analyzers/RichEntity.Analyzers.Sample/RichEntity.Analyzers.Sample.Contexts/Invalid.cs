using Microsoft.EntityFrameworkCore;

namespace RichEntity.Analyzers.Sample;

public class Invalid
{
    public class Context : DbContext
    {
        private const string Children = "_childrens";
        private const string Name = "name";
        private const string Father = "father";
        private const string Friends = "_friends";

        public DbSet<Types.Invalid.Person> Persons { get; private set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var eb = modelBuilder.Entity<Types.Invalid.Person>();
            var nb = eb.Navigation(p => p.Children);

            modelBuilder.Entity<Types.Invalid.Person>().Navigation(p => p.Children).HasField("_childrens");
            modelBuilder.Entity<Types.Invalid.Person>().Navigation(p => p.Children).HasField(Children);
            modelBuilder.Entity<Types.Invalid.Person>().Property("name");
            modelBuilder.Entity<Types.Invalid.Person>().Property(Name);
            modelBuilder.Entity<Types.Invalid.Person>().HasOne("father");
            modelBuilder.Entity<Types.Invalid.Person>().HasOne(Father);
            modelBuilder.Entity<Types.Invalid.Person>().HasMany("friends");
            modelBuilder.Entity<Types.Invalid.Person>().HasMany(Friends);
            modelBuilder.Entity<Types.Invalid.Person>().OwnsMany("adad", "adaadd");
            modelBuilder.Entity<Types.Invalid.Person>().OwnsOne<Types.Invalid.Person>("a", "b");
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
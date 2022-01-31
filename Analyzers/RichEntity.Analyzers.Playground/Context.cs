using Microsoft.EntityFrameworkCore;

namespace RichEntity.Analyzers.Playground;

public class Program
{
    public static void Main(string[] args) { }
}

public class Person
{
    private string _name;

    public Person(string name, Person? parent = null)
    {
        _name = name;
        Parent = parent;
    }

    public string Name => _name;
    public Person? Parent { get; init; }
}

public class Context : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().Navigation(e => e.Parent).HasField("_name");
        modelBuilder.Entity<Person>().Property("_name");
    }
}
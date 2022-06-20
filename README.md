# RichEntity [![Nuget](https://img.shields.io/nuget/v/RichEntity?style=flat-square)](https://www.nuget.org/packages/RichEntity)

A library for reducing boilerplate when defining entities.
It is using Roslyn's Source Generators to generate the code
that is needed for entity definition.

## Usage 
To generate an entity, you will need to add an `IEntity<TIdentifier>`
to base list of your entity class.

```csharp
public partial class Sample : IEntity<int> { }
```

The library will generate this code in the background.

```csharp
public partial class Sample : IEquatable<Sample>
{
    public Int32 Id { get; protected init; }

    protected Sample(Int32 id)
    {
        Id = id;
    }

#nullable enable
    public bool Equals(Sample? other)
#nullable restore
    {
        return other?.Id.Equals(Id) ?? false;
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as Sample);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

You can customize generated code via attributes.

### ConfigureConstructorsAttribute
To configure generated entity constructors decorate type with 
`ConfigureConstructorsAttribute`.

#### Settings:
- ParametrizedConstructorAccessibility\
    Accessibility of parameterized constructor (`Entity(TIdentity id)`).

```csharp
[ConfigureConstructors(ParametrizedConstructorAccessibility = Accessibility.Public)]
public partial class Sample : IEntity<int> { }
```

```csharp
public partial class Sample : IEquatable<Sample>
{
    public Int32 Id { get; protected init; }

    public Sample(Int32 id)
    {
        Id = id;
    }

#nullable enable
    public bool Equals(Sample? other)
#nullable restore
    {
        return other?.Id.Equals(Id) ?? false;
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as Sample);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

### ConfigureIdAttribute
To configure generated identifier decorate type with `ConfigureIdAttribute`.

#### Settings:
- SetterAccessibility \
  Accessibility of `Id` property setter.
- SetterType \
  Type of `Id` property setter (`set` / `init`).

```csharp
[ConfigureId(SetterAccessibility = Accessibility.Internal, SetterType = SetterType.Set)]
public partial class Sample : IEntity<int> { }
```

```csharp
public partial class Sample : IEquatable<Sample>
{
    public Int32 Id { get; internal set; }

    protected Sample(Int32 id)
    {
        Id = id;
    }

#nullable enable
    public bool Equals(Sample? other)
#nullable restore
    {
        return other?.Id.Equals(Id) ?? false;
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as Sample);
    }

    public override int GetHashCode() => Id.GetHashCode();
}
```

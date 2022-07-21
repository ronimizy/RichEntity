# RichEntity [![Nuget](https://img.shields.io/nuget/v/RichEntity?style=flat-square)](https://www.nuget.org/packages/RichEntity)

A library for reducing boilerplate when defining entities.
It is using Roslyn's Source Generators to generate the code
that is needed for entity definition.

## NOTE!
If your application failing to compile due to dotnet failing to locate `Microsoft.CodeAnalysis` package, update your dotnet sdk to the latest version.

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

#pragma warning disable CS8618
    protected Sample(Int32 id)
#pragma warning restore CS8618
    {
        Id = id;
    }

#pragma warning disable CS8618
    protected Sample()
#pragma warning restore CS8618
    {
    }

#nullable enable
    public bool Equals(Sample? other)
#nullable restore
    {
        return (other?.Id.Equals(Id) ?? false);
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as Sample);
    }

    public override int GetHashCode() => (Id).GetHashCode();
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
    public Int32 Id { get; init; }

#pragma warning disable CS8618
    protected Sample(Int32 id)
#pragma warning restore CS8618
    {
        Id = id;
    }

#pragma warning disable CS8618
    protected Sample()
#pragma warning restore CS8618
    {
    }

#nullable enable
    public bool Equals(Sample? other)
#nullable restore
    {
        return (other?.Id.Equals(Id) ?? false);
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as Sample);
    }

    public override int GetHashCode() => (Id).GetHashCode();
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

#pragma warning disable CS8618
    protected Sample(Int32 id)
#pragma warning restore CS8618
    {
        Id = id;
    }

#pragma warning disable CS8618
    protected Sample()
#pragma warning restore CS8618
    {
    }

#nullable enable
    public bool Equals(Sample? other)
#nullable restore
    {
        return (other?.Id.Equals(Id) ?? false);
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as Sample);
    }

    public override int GetHashCode() => (Id).GetHashCode();
}
```

## Composite key entities
For entities with composite key, you can use an `IEntity` interface with `KeyProperty` attribute. \
It will generate a constructor, an `Equals` implemetation and a `GetHashCode` implementation for that property. \
For properties of type `IEntity<TIdentifier>` the property of type `TIdentifier` will be generated as well 
(and it will be used in all implementations listed above).

```csharp
public partial class C : IEntity
{
    [KeyProperty]
    public Sample Sample { get; init; }

    [KeyProperty]
    public int Composite { get; init; }
}
```

This will generate:

```csharp
public partial class C : IEquatable<C>
{
    public Int32 SampleId { get; protected init; }

#pragma warning disable CS8618
    protected C(Int32 composite, Int32 sampleId)
#pragma warning restore CS8618
    {
        Composite = composite;
        SampleId = sampleId;
    }

#pragma warning disable CS8618
    protected C()
#pragma warning restore CS8618
    {
    }

#nullable enable
    public bool Equals(C? other)
#nullable restore
    {
        return (other?.Composite.Equals(Composite) ?? false) && (other?.SampleId.Equals(SampleId) ?? false);
    }

#nullable enable
    public override bool Equals(object? other)
#nullable restore
    {
        return Equals(other as C);
    }

    public override int GetHashCode() => (Composite, SampleId).GetHashCode();
}
```

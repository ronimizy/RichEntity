using System;
using RichEntity.Annotations;

namespace RichEntity.Analyzers.Sample.Generation.Entity;

[ConfigureId(SetterAccessibility = Accessibility.Internal, SetterType = SetterType.Set)]
public partial class Sample : IEntity<int> { }

public readonly partial record struct B : IEntity<Guid>
{
    public B() : this(Guid.NewGuid()) { }
}
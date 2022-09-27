using System;
using RichEntity.Annotations;

namespace RichEntity.Analyzers.Sample.Generation.Entity;

[ConfigureId(SetterAccessibility = Accessibility.Internal, SetterType = SetterType.Set)]
public partial class Sample : IEntity<int> { }

public partial class SampleDerived : Sample { }

public readonly partial record struct B : IEntity<Guid>
{
    public B() : this(Guid.NewGuid()) { }
}

public partial class C : IEntity
{
    [KeyProperty]
    public Sample Sample { get; init; }

    [KeyProperty]
    public B Composite { get; init; }
}

public partial class D : IEntity
{
    public D(C c) : this(sampleId: c.SampleId, compositeId: c.CompositeId)
    {
        C = c;
    }

    [KeyProperty]
    public C C { get; set; }
}
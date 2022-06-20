namespace RichEntity.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ConfigureConstructorsAttribute : Attribute
{
    public Accessibility ParametrizedConstructorAccessibility { get; set; }
}
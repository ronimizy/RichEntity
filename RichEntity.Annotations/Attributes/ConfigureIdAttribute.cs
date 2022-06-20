namespace RichEntity.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ConfigureIdAttribute : Attribute
{
    public Accessibility SetterAccessibility { get; set; }
    public SetterType SetterType { get; set; }
}
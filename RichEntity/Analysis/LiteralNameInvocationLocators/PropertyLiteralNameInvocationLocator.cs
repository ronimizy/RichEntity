namespace RichEntity.Analysis.LiteralNameInvocationLocators;

public class PropertyLiteralNameInvocationLocator : Base.MultipleArgumentLiteralNameInvocationLocator
{
    public override string MemberType => "Property";
    protected override string ParameterName => "propertyName";

    protected override bool IsNameComplies(string methodName)
        => methodName is "Property";
}
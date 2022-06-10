namespace RichEntity.Analysis.LiteralNameInvocationLocators;

public class NavigationLiteralNameInvocationLocator : Base.MultipleArgumentLiteralNameInvocationLocator
{
    public override string MemberType => "Navigation";
    protected override string ParameterName => "navigationName";

    protected override bool IsNameComplies(string methodName)
        => methodName is "HasOne" || methodName is "HasMany";
}
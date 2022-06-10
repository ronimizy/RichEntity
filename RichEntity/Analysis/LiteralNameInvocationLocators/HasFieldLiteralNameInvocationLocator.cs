namespace RichEntity.Analysis.LiteralNameInvocationLocators;

public class HasFieldLiteralNameInvocationLocator : Base.SingleStringArgumentLiteralNameInvocationLocator
{
    public override string MemberType => "Field";

    protected override bool IsNameComplies(string methodName)
        => methodName.Equals("HasField");
}
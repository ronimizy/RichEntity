namespace RichEntity.Analyzers.LiteralNameInvocationLocators
{
    public class PropertyLiteralNameInvocationLocator : Base.MultipleArgumentLiteralNameInvocationLocator
    {
        public override string MemberType => "Property";

        protected override bool IsNameComplies(string methodName)
            => methodName is "Property";
    }
}
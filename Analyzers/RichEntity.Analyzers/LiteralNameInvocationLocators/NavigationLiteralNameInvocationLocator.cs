namespace RichEntity.Analyzers.LiteralNameInvocationLocators
{
    public class NavigationLiteralNameInvocationLocator : Base.MultipleArgumentLiteralNameInvocationLocator
    {
        public override string MemberType => "Navigation";

        protected override bool IsNameComplies(string methodName)
            => methodName is "HasOne" || methodName is "HasMany";
    }
}
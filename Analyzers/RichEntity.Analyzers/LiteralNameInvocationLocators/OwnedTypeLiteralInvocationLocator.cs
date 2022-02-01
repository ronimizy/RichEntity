using RichEntity.Analyzers.LiteralNameInvocationLocators.Base;

namespace RichEntity.Analyzers.LiteralNameInvocationLocators
{
    public class OwnedTypeLiteralInvocationLocator : MultipleArgumentLiteralNameInvocationLocator
    {
        public override string MemberType => "Owned property";
        protected override string ParameterName => "navigationName";

        protected override bool IsNameComplies(string methodName)
            => methodName is "OwnsOne" || methodName is "OwnsMany";
    }
}
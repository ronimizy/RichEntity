using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Analyzers.Extensions;

namespace RichEntity.Analyzers.LiteralNameInvocationLocators.Base
{
    public abstract class MultipleArgumentLiteralNameInvocationLocator : ILiteralNameInvocationLocator
    {
        public abstract string MemberType { get; }

        public bool IsInvocationOperationRelevant(
            IInvocationOperation invocationOperation, OperationAnalysisContext context)
        {
            if (!IsNameComplies(invocationOperation.TargetMethod.Name))
                return false;

            var stringType = context.Compilation.GetTypeByMetadataName("System.String");

            if (stringType is null)
                return false;

            return invocationOperation.Arguments.Any(a => stringType.Equals(a.Value.Type));
        }

        public IArgumentOperation GetRelevantArgument(
            ImmutableArray<IArgumentOperation> arguments, OperationAnalysisContext context)
        {
            var stringType = context.Compilation.GetTypeByMetadataName("System.String")!;
            return arguments.Single(a => stringType.Equals(a.Value.Type));
        }

        public bool ContainsMember(
            ImmutableArray<ISymbol> memberSymbols, string memberName, OperationAnalysisContext context)
            => memberSymbols.ContainsPropertyFieldCalled(memberName);

        protected abstract bool IsNameComplies(string methodName);
    }
}
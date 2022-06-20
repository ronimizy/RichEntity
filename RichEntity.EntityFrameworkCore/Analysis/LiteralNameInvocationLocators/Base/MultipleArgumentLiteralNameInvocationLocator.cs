using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Extensions;

namespace RichEntity.EntityFrameworkCore.Analysis.LiteralNameInvocationLocators.Base;

public abstract class MultipleArgumentLiteralNameInvocationLocator : ILiteralNameInvocationLocator
{
    public abstract string MemberType { get; }
    protected abstract string ParameterName { get; }

    public bool IsInvocationOperationRelevant(
        IInvocationOperation invocationOperation, OperationAnalysisContext context)
    {
        if (!IsNameComplies(invocationOperation.TargetMethod.Name))
            return false;

        var stringType = context.Compilation.GetTypeByMetadataName("System.String");

        if (stringType is null)
            return false;

        return invocationOperation.Arguments.Any(a => stringType.EqualsDefault(a.Value.Type));
    }

    public IArgumentOperation GetRelevantArgument(
        ImmutableArray<IArgumentOperation> arguments,
        ImmutableArray<IParameterSymbol> parameters,
        OperationAnalysisContext context)
    {
        var stringType = context.Compilation.GetTypeByMetadataName("System.String")!;

        return arguments
            .Where(a => stringType.EqualsDefault(a.Value.Type))
            .Select((a, i) => (Argument: a, i))
            .Single(t => parameters[t.i].Name.Equals(ParameterName)).Argument;
    }

    public bool ContainsMember(
        ImmutableArray<ISymbol> memberSymbols, string memberName, OperationAnalysisContext context)
        => memberSymbols.ContainsPropertyFieldCalled(memberName);

    protected abstract bool IsNameComplies(string methodName);
}
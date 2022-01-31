using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Analyzers.EntityTypeSymbolProviders;
using RichEntity.Analyzers.Utility;

namespace RichEntity.Analyzers.Extensions
{
    public static class OperationExtensions
    {
        public static ITypeSymbol? GetOperationUnderlyingEntityType(this IOperation operation, Compilation compilation)
        {
            var providers = AssemblyScanner.GetInstances<IEntityTypeSymbolProvider>();

            while (true)
            {
                foreach (var provider in providers)
                {
                    if (provider.TryGetTypeSymbol(operation, compilation, out var symbol))
                    {
                        return symbol;
                    }
                }

                if (operation is IInvocationOperation { Instance: { } instance })
                {
                    operation = instance;
                    continue;
                }

                return null;
            }
        }
    }
}
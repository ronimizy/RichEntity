using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using RichEntity.Core.EntityTypeSymbolProviders.Base;
using RichEntity.Core.Utility;

namespace RichEntity.Core.Extensions
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
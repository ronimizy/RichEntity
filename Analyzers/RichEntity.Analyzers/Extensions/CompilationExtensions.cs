using System.Linq;
using Microsoft.CodeAnalysis;

namespace RichEntity.Analyzers.Extensions
{
    public static class CompilationExtensions
    {
        public static bool IsEntityFrameworkCoreReferenced(this Compilation compilation)
            => compilation.References.Any(r => r.Display?.Contains("Microsoft.EntityFrameworkCore") ?? false);
    }
}
using Microsoft.CodeAnalysis.CSharp;
using RichEntity.Annotations;

namespace RichEntity.Extensions;

public static class SetterTypeExtensions
{
    public static SyntaxKind ToSyntaxKind(this SetterType value, SyntaxKind defaultValue)
    {
        return value switch
        {
            SetterType.Init => SyntaxKind.InitAccessorDeclaration,
            SetterType.Set => SyntaxKind.SetAccessorDeclaration,
            _ => defaultValue,
        };
    }
}
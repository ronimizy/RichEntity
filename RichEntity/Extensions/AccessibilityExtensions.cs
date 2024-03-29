using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MyAccessibility = RichEntity.Annotations.Accessibility;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RichEntity.Extensions;

public static class AccessibilityExtensions
{
    public static SyntaxTokenList ToSyntaxTokenList(this Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => TokenList(Token(SyntaxKind.PublicKeyword)),
        Accessibility.Private => TokenList(Token(SyntaxKind.PrivateKeyword)),
        Accessibility.Protected => TokenList(Token(SyntaxKind.ProtectedKeyword)),
        Accessibility.Internal => TokenList(Token(SyntaxKind.InternalKeyword)),
        Accessibility.ProtectedAndInternal => TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword)),
        Accessibility.ProtectedOrInternal => TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ProtectedKeyword)),
        _ => TokenList(),
    };

    public static SyntaxKind ToSyntaxKind(this MyAccessibility accessibility, SyntaxKind defaultValue = default)
    {
        return accessibility switch
        {
            MyAccessibility.Private => SyntaxKind.PrivateKeyword,
            MyAccessibility.Protected => SyntaxKind.ProtectedKeyword,
            MyAccessibility.Internal => SyntaxKind.InternalKeyword,
            MyAccessibility.Public => SyntaxKind.PublicKeyword,
            _ => defaultValue,
        };
    }
}
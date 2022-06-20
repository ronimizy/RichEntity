using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Generation.Entity.Commands;

public record struct TypeCheckingCommand(
    TypeDeclarationSyntax Syntax,
    INamedTypeSymbol Symbol,
    ITypeSymbol IdentifierSymbol,
    GeneratorExecutionContext Context);
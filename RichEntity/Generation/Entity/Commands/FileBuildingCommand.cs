using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RichEntity.Generation.Entity.Commands;

public record struct FileBuildingCommand(
    TypeDeclarationSyntax Syntax,
    INamedTypeSymbol Symbol,
    ITypeSymbol IdentifierSymbol,
    CompilationUnitSyntax Root,
    GeneratorExecutionContext Context);
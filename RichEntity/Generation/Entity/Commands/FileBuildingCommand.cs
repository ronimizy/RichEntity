using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Models;

namespace RichEntity.Generation.Entity.Commands;

public record struct FileBuildingCommand(
    TypeDeclarationSyntax Syntax,
    INamedTypeSymbol Symbol,
    IReadOnlyCollection<Identifier> Identifiers,
    CompilationUnitSyntax Root,
    GeneratorExecutionContext Context);
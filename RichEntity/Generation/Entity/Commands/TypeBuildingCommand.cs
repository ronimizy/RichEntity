using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RichEntity.Generation.Entity.Models;

namespace RichEntity.Generation.Entity.Commands;

public record struct TypeBuildingCommand(
    TypeDeclarationSyntax Syntax,
    INamedTypeSymbol Symbol,
    IReadOnlyList<Identifier> Identifiers,
    TypeDeclarationSyntax Root,
    Compilation Compilation);
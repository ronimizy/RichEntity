using Microsoft.CodeAnalysis;

namespace RichEntity.Generation.Entity.Commands;

public record GetIdentifiersCommand(INamedTypeSymbol Symbol, Compilation Compilation);
using Microsoft.CodeAnalysis;

namespace RichEntity.Generation.Entity.Commands;

public record struct GetIdentifiersCommand(INamedTypeSymbol Symbol, Compilation Compilation);
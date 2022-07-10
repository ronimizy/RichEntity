using Microsoft.CodeAnalysis;

namespace RichEntity.Generation.Entity.Models;

public record struct Identifier(string CapitalizedName, string LowercasedName, ITypeSymbol Type);
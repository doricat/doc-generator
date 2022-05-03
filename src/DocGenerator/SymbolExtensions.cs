using Microsoft.CodeAnalysis;

namespace DocGenerator
{
    public static class SymbolExtensions
    {
        public static string GetFullTypeName(this ISymbol symbol)
        {
            var definition = symbol.OriginalDefinition;
            return $"{definition.ContainingNamespace.ToDisplayString()}.{definition.MetadataName}";
        }
    }
}
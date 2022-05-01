using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGenerator
{
    public static class PropertyDeclarationSyntaxExtensions
    {
        public static bool IsPublicProperty(this PropertyDeclarationSyntax syntax)
        {
            return syntax.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword) || x.IsKind(SyntaxKind.InternalKeyword));
        }

        public static bool IncludeGetterAccessor(this PropertyDeclarationSyntax syntax)
        {
            return syntax?.AccessorList?.Accessors.Count >= 1 && syntax.AccessorList.Accessors.Any(x => x.Keyword.IsKind(SyntaxKind.GetKeyword));
        }
    }
}
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGenerator
{
    public static class PredefinedTypeSyntaxExtensions
    {
        public static string GetKeyword(this PredefinedTypeSyntax syntax)
        {
            if (syntax is PredefinedTypeSyntax typeSyntax)
            {
                return typeSyntax.Keyword.ValueText;
            }

            throw new InvalidOperationException();
        }

        public static object GenerateDefaultValue(this PredefinedTypeSyntax syntax)
        {
            return KeywordDefaultValue.GetValue(GetKeyword(syntax));
        }
    }
}
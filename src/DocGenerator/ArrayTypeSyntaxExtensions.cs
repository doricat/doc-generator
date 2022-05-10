using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGenerator
{
    public static class ArrayTypeSyntaxExtensions
    {
        public static object GenerateDefaultValue(this ArrayTypeSyntax syntax, Compilation compilation)
        {
            object obj = null;
            switch (syntax.ElementType)
            {
                case PredefinedTypeSyntax predefinedTypeSyntax:
                    obj = predefinedTypeSyntax.GenerateDefaultValue();
                    break;
                case IdentifierNameSyntax identifierNameSyntax:
                    obj = identifierNameSyntax.GenerateDefaultValue(compilation);
                    break;
                case GenericNameSyntax genericNameSyntax:
                    obj = genericNameSyntax.GenerateDefaultValue(compilation);
                    break;
                case ArrayTypeSyntax arrayTypeSyntax2:
                    obj = GenerateDefaultValue(arrayTypeSyntax2, compilation);
                    break;
            }

            return obj != null ? new[] { obj } : Array.Empty<object>();
        }
    }
}
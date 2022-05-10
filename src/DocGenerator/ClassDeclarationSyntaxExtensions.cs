using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGenerator
{
    public static class ClassDeclarationSyntaxExtensions
    {
        public static IEnumerable GeneratePropertyCollection(this ClassDeclarationSyntax syntax, Compilation compilation)
        {
            var result = new Dictionary<string, object>();
            GeneratePropertyCollection(syntax, compilation, result);
            return result;
        }

        public static void GeneratePropertyCollection(this ClassDeclarationSyntax syntax, Compilation compilation, IDictionary<string, object> result)
        {
            GeneratePropertyCollection(syntax, compilation, result, null);
        }

        public static void GeneratePropertyCollection(this ClassDeclarationSyntax syntax, Compilation compilation, IDictionary<string, object> result, TypeArgumentListSyntax typeArgumentListSyntax)
        {
            ParseBaseTypeSyntax(syntax, compilation, result);

            var members = syntax
                .Members
                .Where(x => x is PropertyDeclarationSyntax)
                .Cast<PropertyDeclarationSyntax>()
                .Where(x => x.IsPublicProperty() && x.IncludeGetterAccessor())
                .ToList();

            foreach (var property in members)
            {
                var identifier = property.Identifier.ValueText;
                if (result.ContainsKey(identifier))
                {
                    continue;
                }

                switch (property.Type)
                {
                    case PredefinedTypeSyntax predefinedTypeSyntax:
                        result.Add(identifier, predefinedTypeSyntax.GenerateDefaultValue());
                        break;
                    case IdentifierNameSyntax identifierNameSyntax:
                        result.Add(identifier, identifierNameSyntax.GenerateDefaultValue(compilation, syntax.TypeParameterList, typeArgumentListSyntax));
                        break;
                    case NullableTypeSyntax _:
                        result.Add(identifier, null);
                        break;
                    case GenericNameSyntax genericNameSyntax:
                        result.Add(identifier, genericNameSyntax.GenerateDefaultValue(compilation));
                        break;
                    case ArrayTypeSyntax arrayTypeSyntax:
                        result.Add(identifier, arrayTypeSyntax.GenerateDefaultValue(compilation));
                        break;
                }
            }
        }

        private static void ParseBaseTypeSyntax(ClassDeclarationSyntax syntax, Compilation compilation, IDictionary<string, object> result)
        {
            if (syntax.BaseList != null)
            {
                foreach (var baseTypeSyntax in syntax.BaseList.Types)
                {
                    baseTypeSyntax.GeneratePropertyCollection(compilation, result);
                }
            }
        }
    }
}
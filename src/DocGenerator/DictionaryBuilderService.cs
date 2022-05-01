using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace DocGenerator
{
    [Export(typeof(IDictionaryBuilderService))]
    public class DictionaryBuilderService : IDictionaryBuilderService
    {
        public IDictionary<string, object> GenerateDictionary(SyntaxContext context)
        {
            var result = new Dictionary<string, object>();
            var syntax = context.Syntax;
            var semanticModel = ThreadHelper.JoinableTaskFactory.Run(() => context.Document.GetSemanticModelAsync());

            switch (syntax)
            {
                case ClassDeclarationSyntax classDeclarationSyntax:
                    ParseClassDeclarationSyntax(classDeclarationSyntax, semanticModel, result);
                    break;
                case IdentifierNameSyntax identifierNameSyntax:
                    ParseIdentifierNameSyntax(identifierNameSyntax, semanticModel, result);
                    break;
                case GenericNameSyntax genericNameSyntax:
                    ParseGenericNameSyntax(genericNameSyntax, semanticModel, result);
                    break;
            }

            return result;
        }

        protected void ParseClassDeclarationSyntax(ClassDeclarationSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            var members = syntax
                .Members
                .Where(x => x is PropertyDeclarationSyntax)
                .Cast<PropertyDeclarationSyntax>()
                .Where(x => x.IsPublicProperty() && x.IncludeGetterAccessor())
                .ToList();

            foreach (var property in members)
            {
                var identifier = property.Identifier.ValueText;
                switch (property.Type)
                {
                    case PredefinedTypeSyntax predefinedTypeSyntax:
                        break;
                    case IdentifierNameSyntax identifierNameSyntax:
                        break;
                    case NullableTypeSyntax nullableTypeSyntax:
                        break;
                    case GenericNameSyntax genericNameSyntax:
                        break;
                    case ArrayTypeSyntax arrayTypeSyntax:
                        break;
                }
            }

            throw new NotImplementedException();
        }

        protected void ParseIdentifierNameSyntax(IdentifierNameSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            throw new NotImplementedException();
        }

        protected void ParseGenericNameSyntax(GenericNameSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            throw new NotImplementedException();
        }
    }
}
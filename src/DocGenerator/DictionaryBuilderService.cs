﻿using System;
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
            ParseBaseTypeSyntax(syntax, semanticModel, result);

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
                        result.Add(identifier, ParsePredefinedTypeProperty(property, predefinedTypeSyntax));
                        break;
                    case IdentifierNameSyntax identifierNameSyntax:
                        result.Add(identifier, ParseIdentifierNameProperty(property, identifierNameSyntax, semanticModel));
                        break;
                    case NullableTypeSyntax nullableTypeSyntax:
                        result.Add(identifier, ParseNullableTypeProperty(property, nullableTypeSyntax));
                        break;
                    case GenericNameSyntax genericNameSyntax:
                        result.Add(identifier, ParseGenericNameProperty(property, genericNameSyntax, semanticModel));
                        break;
                    case ArrayTypeSyntax arrayTypeSyntax:
                        result.Add(identifier, ParseArrayTypeProperty(property, arrayTypeSyntax, semanticModel));
                        break;
                }
            }
        }

        protected void ParseIdentifierNameSyntax(IdentifierNameSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(syntax);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
            {
                return;
            }

            foreach (var location in symbolInfo.Symbol.Locations)
            {
                if (location.SourceTree != null)
                {
                    var root = ThreadHelper.JoinableTaskFactory.Run(() => location.SourceTree.GetRootAsync());
                    var node = root.FindNode(location.SourceSpan);
                    if (node is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        ParseClassDeclarationSyntax(classDeclarationSyntax, semanticModel, result);
                    }
                }
            }
        }

        protected void ParseGenericNameSyntax(GenericNameSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            throw new NotImplementedException();
        }

        protected void ParseBaseTypeSyntax(ClassDeclarationSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            if (syntax.BaseList == null)
            {
                return;
            }

            foreach (var baseTypeSyntax in syntax.BaseList.Types)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(baseTypeSyntax.Type);
                if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
                {
                    continue;
                }

                foreach (var location in symbolInfo.Symbol.Locations)
                {
                    if (location.SourceTree != null)
                    {
                        var root = ThreadHelper.JoinableTaskFactory.Run(() => location.SourceTree.GetRootAsync());
                        var node = root.FindNode(location.SourceSpan);
                        if (node is ClassDeclarationSyntax classDeclarationSyntax)
                        {
                            ParseClassDeclarationSyntax(classDeclarationSyntax, semanticModel, result);
                        }
                    }
                }
            }
        }

        protected object ParsePredefinedTypeProperty(PropertyDeclarationSyntax _, PredefinedTypeSyntax predefinedTypeSyntax)
        {
            return KeywordDefaultValue.GetValue(predefinedTypeSyntax.GetKeyword());
        }

        protected object ParseIdentifierNameProperty(PropertyDeclarationSyntax _, IdentifierNameSyntax identifierNameSyntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifierNameSyntax);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
            {
                return null;
            }

            var dict = new Dictionary<string, object>();
            foreach (var location in symbolInfo.Symbol.Locations)
            {
                if (location.SourceTree != null)
                {
                    var root = ThreadHelper.JoinableTaskFactory.Run(() => location.SourceTree.GetRootAsync());
                    var node = root.FindNode(location.SourceSpan);
                    if (node is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        ParseClassDeclarationSyntax(classDeclarationSyntax, semanticModel, dict);
                    }
                }
                else
                {
                    var fullName = symbolInfo.Symbol.ToDisplayString();
                    var type = Type.GetType(fullName);
                    if (type != null)
                    {
                        return Activator.CreateInstance(type);
                    }
                }
            }

            return dict;
        }

        protected object ParseNullableTypeProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, NullableTypeSyntax nullableTypeSyntax)
        {
            return null;
        }

        protected object ParseGenericNameProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, GenericNameSyntax genericNameSyntax, SemanticModel semanticModel)
        {
            return null;
        }

        protected object ParseArrayTypeProperty(PropertyDeclarationSyntax propertyDeclarationSyntax, ArrayTypeSyntax arrayTypeSyntax, SemanticModel semanticModel)
        {
            object obj = null;
            switch (arrayTypeSyntax.ElementType)
            {
                case PredefinedTypeSyntax predefinedTypeSyntax:
                    obj = KeywordDefaultValue.GetValue(predefinedTypeSyntax.GetKeyword());
                    break;
                case IdentifierNameSyntax identifierNameSyntax:
                    obj = ParseIdentifierNameProperty(propertyDeclarationSyntax, identifierNameSyntax, semanticModel);
                    break;
                case GenericNameSyntax genericNameSyntax:
                    obj = ParseGenericNameProperty(propertyDeclarationSyntax, genericNameSyntax, semanticModel);
                    break;
                case ArrayTypeSyntax arrayTypeSyntax2:
                    obj = ParseArrayTypeProperty(propertyDeclarationSyntax, arrayTypeSyntax2, semanticModel);
                    break;
            }

            return obj != null ? new[] { obj } : Array.Empty<object>();
        }
    }
}
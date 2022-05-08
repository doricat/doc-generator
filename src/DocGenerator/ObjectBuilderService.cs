using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace DocGenerator
{
    [Export(typeof(IObjectBuilderService))]
    public class ObjectBuilderService : IObjectBuilderService
    {
        public IEnumerable GenerateResult(SyntaxContext context)
        {
            var dict = new Dictionary<string, object>();
            IEnumerable result = null;
            var syntax = context.Syntax;
            var semanticModel = ThreadHelper.JoinableTaskFactory.Run(() => context.Document.GetSemanticModelAsync());

            switch (syntax)
            {
                case ClassDeclarationSyntax classDeclarationSyntax:
                    ParseClassDeclarationSyntax(classDeclarationSyntax, semanticModel, dict);
                    break;
                case IdentifierNameSyntax identifierNameSyntax:
                    ParseIdentifierNameSyntax(identifierNameSyntax, semanticModel, dict);
                    break;
                case GenericNameSyntax genericNameSyntax:
                    result = ParseGenericNameSyntax(genericNameSyntax, semanticModel).GetResult();
                    break;
            }

            return result ?? dict;
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

        protected void ParseClassDeclarationSyntax(ClassDeclarationSyntax syntax, SeparatedSyntaxList<TypeSyntax> arguments, SemanticModel semanticModel, IDictionary<string, object> result)
        {

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

        protected GenericTypeResult ParseGenericNameSyntax(GenericNameSyntax syntax, SemanticModel semanticModel)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(syntax);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
            {
                return null;
            }

            var dict = new Dictionary<string, object>();
            var isCollection = false;
            foreach (var location in symbolInfo.Symbol.Locations)
            {
                if (location.SourceTree != null)
                {
                    var root = ThreadHelper.JoinableTaskFactory.Run(() => location.SourceTree.GetRootAsync());
                    var node = root.FindNode(location.SourceSpan);
                    if (node is ClassDeclarationSyntax classDeclarationSyntax)
                    {
                        ParseClassDeclarationSyntax(classDeclarationSyntax, syntax.TypeArgumentList.Arguments, semanticModel, dict);
                    }
                }
                else
                {
                    var typeName = symbolInfo.Symbol.GetFullTypeName();
                    var type = Type.GetType(typeName);
                    if (type == null || !type.IsEnumerable())
                    {
                        continue;
                    }

                    isCollection = true;
                }
            }

            return new GenericTypeResult(dict, isCollection);
        }

        protected void ParseBaseTypeSyntax(ClassDeclarationSyntax syntax, SemanticModel semanticModel, IDictionary<string, object> result)
        {
            if (syntax.BaseList == null)
            {
                return;
            }

            foreach (var baseTypeSyntax in syntax.BaseList.Types)
            {
                var model = semanticModel.Compilation.GetSemanticModel(baseTypeSyntax.Type.SyntaxTree);
                var symbolInfo = model.GetSymbolInfo(baseTypeSyntax.Type);
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
                            ParseClassDeclarationSyntax(classDeclarationSyntax, model, result);
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
            var symbolInfo = semanticModel.Compilation.GetSemanticModel(identifierNameSyntax.SyntaxTree).GetSymbolInfo(identifierNameSyntax);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
            {
                return null;
            }

            var dict = new Dictionary<string, object>();
            foreach (var location in symbolInfo.Symbol.Locations)
            {
                if (location.SourceTree == null)
                {
                    return null;
                }

                var root = ThreadHelper.JoinableTaskFactory.Run(() => location.SourceTree.GetRootAsync());
                var node = root.FindNode(location.SourceSpan);
                if (node is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    ParseClassDeclarationSyntax(classDeclarationSyntax, semanticModel, dict);
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
            return ParseGenericNameSyntax(genericNameSyntax, semanticModel).GetResult();
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

        protected class GenericTypeResult
        {
            public GenericTypeResult(IDictionary<string, object> result, bool isCollection)
            {
                Result = result ?? throw new ArgumentNullException(nameof(result));
                IsCollection = isCollection;
            }

            public IDictionary<string, object> Result { get; }

            public bool IsCollection { get; }

            public IEnumerable GetResult()
            {
                return IsCollection ? (IEnumerable) new[] {Result} : Result;
            }
        }
    }
}
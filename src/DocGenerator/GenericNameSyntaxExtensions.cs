using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace DocGenerator
{
    public static class GenericNameSyntaxExtensions
    {
        public static IEnumerable GeneratePropertyCollection(this GenericNameSyntax syntax, Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var symbolInfo = semanticModel.GetSymbolInfo(syntax);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
            {
                return Array.Empty<object>();
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
                        classDeclarationSyntax.GeneratePropertyCollection(compilation, dict, syntax.TypeArgumentList);
                    }
                }
                else
                {
                    var typeName = symbolInfo.Symbol.GetFullTypeName();
                    var type = Type.GetType(typeName);
                    if (type == null || !type.IsEnumerable() || syntax.TypeArgumentList.Arguments.Count > 1)
                    {
                        continue;
                    }

                    isCollection = true;
                    syntax.TypeArgumentList.Arguments[0].GeneratePropertyCollection(compilation, dict);
                }
            }

            return isCollection ? (IEnumerable) new[] {dict} : dict;
        }

        public static object GenerateDefaultValue(this GenericNameSyntax syntax, Compilation compilation)
        {
            return GeneratePropertyCollection(syntax, compilation);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace DocGenerator
{
    public static class IdentifierNameSyntaxExtensions
    {
        public static IEnumerable GeneratePropertyCollection(this IdentifierNameSyntax syntax, Compilation compilation)
        {
            return GeneratePropertyCollection(syntax, compilation, null, null);
        }

        public static IEnumerable GeneratePropertyCollection(this IdentifierNameSyntax syntax, Compilation compilation, TypeParameterListSyntax typeParameterListSyntax,
            TypeArgumentListSyntax typeArgumentListSyntax)
        {
            var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            var symbolInfo = semanticModel.GetSymbolInfo(syntax);
            if (symbolInfo.Symbol == null || symbolInfo.Symbol.Locations == null)
            {
                return Array.Empty<object>();
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
                        classDeclarationSyntax.GeneratePropertyCollection(compilation, dict);
                    }
                    else if (node is TypeParameterSyntax typeParameterSyntax)
                    {
                        if (typeParameterListSyntax != null && typeArgumentListSyntax != null)
                        {
                            for (var i = 0; i < typeParameterListSyntax.Parameters.Count; i++)
                            {
                                var paramSyntax = typeParameterListSyntax.Parameters[i];
                                if (paramSyntax.Identifier.ValueText == typeParameterSyntax.Identifier.ValueText)
                                {
                                    var typeSyntax = typeArgumentListSyntax.Arguments[i];
                                    typeSyntax.GeneratePropertyCollection(compilation, dict);
                                }
                            }
                        }
                    }
                }
            }

            return dict;
        }

        public static object GenerateDefaultValue(this IdentifierNameSyntax syntax, Compilation compilation)
        {
            return GeneratePropertyCollection(syntax, compilation, null, null);
        }

        public static object GenerateDefaultValue(this IdentifierNameSyntax syntax, Compilation compilation, TypeParameterListSyntax typeParameterListSyntax,
            TypeArgumentListSyntax typeArgumentListSyntax)
        {
            return GeneratePropertyCollection(syntax, compilation, typeParameterListSyntax, typeArgumentListSyntax);
        }
    }
}
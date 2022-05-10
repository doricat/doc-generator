using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace DocGenerator
{
    public static class BaseTypeSyntaxExtensions
    {
        public static void GeneratePropertyCollection(this BaseTypeSyntax syntax, Compilation compilation, IDictionary<string, object> result)
        {
            var semanticModel = compilation.GetSemanticModel(syntax.Type.SyntaxTree);
            var symbolInfo = semanticModel.GetSymbolInfo(syntax.Type);
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
                        classDeclarationSyntax.GeneratePropertyCollection(compilation, result);
                    }
                }
            }
        }
    }
}
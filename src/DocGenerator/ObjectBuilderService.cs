using System;
using System.Collections;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;

namespace DocGenerator
{
    [Export(typeof(IObjectBuilderService))]
    public class ObjectBuilderService : IObjectBuilderService
    {
        public IEnumerable GenerateResult(SyntaxContext context)
        {
            var semanticModel = ThreadHelper.JoinableTaskFactory.Run(() => context.Document.GetSemanticModelAsync());
            if (semanticModel == null)
            {
                return Array.Empty<object>();
            }

            IEnumerable result;
            var syntax = context.Syntax;
            switch (syntax)
            {
                case ClassDeclarationSyntax classDeclarationSyntax:
                    result = classDeclarationSyntax.GeneratePropertyCollection(semanticModel.Compilation);
                    break;
                case IdentifierNameSyntax identifierNameSyntax:
                    result = identifierNameSyntax.GeneratePropertyCollection(semanticModel.Compilation);
                    break;
                case GenericNameSyntax genericNameSyntax:
                    result = genericNameSyntax.GeneratePropertyCollection(semanticModel.Compilation);
                    break;
                default:
                    result = Array.Empty<object>();
                    break;
            }

            return result;
        }
    }
}
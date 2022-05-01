using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGenerator
{
    public class SyntaxContext
    {
        public SyntaxContext(Document document, SyntaxNode syntax)
        {
            Document = document;
            Syntax = syntax;
        }

        public bool IntendedSyntax => Syntax is ClassDeclarationSyntax classDeclaration
                                      && classDeclaration.TypeParameterList == null
                                      || Syntax is IdentifierNameSyntax
                                      || Syntax is GenericNameSyntax;

        public Document Document { get; }

        public SyntaxNode Syntax { get; }
    }
}
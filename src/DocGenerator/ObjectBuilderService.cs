using System.ComponentModel.Composition;

namespace DocGenerator
{
    [Export(typeof(IObjectBuilderService))]
    public class ObjectBuilderService : IObjectBuilderService
    {
        public GeneratedResult GenerateResult(SyntaxContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
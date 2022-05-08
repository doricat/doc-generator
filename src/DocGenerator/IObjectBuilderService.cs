using System.Collections;

namespace DocGenerator
{
    public interface IObjectBuilderService
    {
        IEnumerable GenerateResult(SyntaxContext context);
    }
}
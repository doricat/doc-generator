using System.Collections.Generic;

namespace DocGenerator
{
    public interface IDictionaryBuilderService
    {
        IDictionary<string, object> GenerateDictionary(SyntaxContext context);
    }
}
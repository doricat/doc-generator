using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace DocGenerator
{
    [Export(typeof(IDictionaryBuilderService))]
    public class DictionaryBuilderService : IDictionaryBuilderService
    {
        public IDictionary<string, object> GenerateDictionary(SyntaxContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}
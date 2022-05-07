using System;
using System.Collections.Generic;

namespace DocGenerator
{
    public class GeneratedResult
    {
        public GeneratedResult(IDictionary<string, object> result, bool isCollection)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
            IsCollection = isCollection;
        }

        public IDictionary<string, object> Result { get;  }

        public bool IsCollection { get;  }

        public object GetResult()
        {
            if (IsCollection)
            {
                return new[] {Result};
            }

            return Result;
        }
    }
}
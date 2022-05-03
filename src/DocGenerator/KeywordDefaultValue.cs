using System;
using System.Collections.Generic;

namespace DocGenerator
{
    public static class KeywordDefaultValue
    {
        private static readonly IDictionary<string, object> Default = new Dictionary<string, object>
        {
            {"sbyte", 0},
            {"byte", 0},
            {"short", 0},
            {"ushort", 0},
            {"int", 0},
            {"uint", 0},
            {"long", 0},
            {"ulong", 0},
            {"char", ' '},
            {"float", 0},
            {"double", 0},
            {"bool", false},
            {"decimal", 0},
            {"string", null},
            {"object", null}
        };

        public static object GetValue(string keyword)
        {
            if (Default.ContainsKey(keyword))
            {
                return Default[keyword];
            }

            throw new InvalidOperationException();
        }
    }
}
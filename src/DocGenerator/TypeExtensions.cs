using System;
using System.Collections;
using System.Linq;

namespace DocGenerator
{
    public static class TypeExtensions
    {
        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(x => x == typeof(IEnumerable));
        }
    }
}
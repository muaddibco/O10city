using System;
using System.Collections.Generic;

namespace O10.Core.ExtensionMethods
{
    public static class GenericExtensionMethods
    {
        public static IEnumerable<T> Join<T>(this T one, IEnumerable<T>? others)
        {

            List<T> list = new List<T> { one };

            if (!(others is null))
            {
                list.AddRange(others);
            }

            return list;
        }
    }
}

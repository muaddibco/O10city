using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace O10.Core.ExtensionMethods
{
    public static class BoolArrayExtensionMethods
    {
        public static BitArray ToBitArray(this bool[] input)
        {
            return new BitArray(input);
        }

        public static BitArray ToBitArray(this IEnumerable<bool> input)
        {
            return new BitArray(input.ToArray());
        }
    }
}

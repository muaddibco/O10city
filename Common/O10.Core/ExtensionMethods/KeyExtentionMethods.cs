using O10.Core.Identity;

namespace O10.Core.ExtensionMethods
{
    public static class KeyExtentionMethods
    {
        public static byte[]? ToByteArray(this IKey key)
        {
            if (key is null)
            {
                return null;
            }

            return key.ArraySegment.ToArray();
        }
    }
}

using O10.Core.Identity;
using System;
using System.Collections.Generic;

namespace O10.Core.ExtensionMethods
{
    public static class DictionaryExtensionMethods
    {
        public static string? Optional(this Dictionary<string, string> dict, string key, string? defaultValue = null)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return defaultValue;
        }

        public static IKey? OptionalKey(this Dictionary<string, string> dict, string key, IIdentityKeyProvider identityKeyProvider)
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (identityKeyProvider is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvider));
            }
            if (dict.ContainsKey(key))
            {
                return identityKeyProvider.GetKey(dict[key].HexStringToByteArray());
            }

            return null;
        }
    }
}

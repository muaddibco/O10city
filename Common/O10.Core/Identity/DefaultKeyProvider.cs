using System;
using System.Collections.Generic;
using O10.Core.Architecture;


namespace O10.Core.Identity
{

    [RegisterExtension(typeof(IIdentityKeyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class DefaultKeyProvider : IIdentityKeyProvider
    {

        public DefaultKeyProvider()
        {

        }
        public string Name => "Default";

        public IEqualityComparer<IKey> GetComparer() => new Key32();

        public IKey? GetKey(Memory<byte> keyBytes)
        {
            if(keyBytes.Length == 0)
            {
                return null;
            }

            if(keyBytes.Length != 32)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 32 bytes");
            }

            return new Key32(keyBytes);
        }
    }
}

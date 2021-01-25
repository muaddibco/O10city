using System;
using System.Collections.Generic;
using O10.Core.Architecture;


namespace O10.Core.Identity
{
    [RegisterExtension(typeof(IIdentityKeyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class DefaultHashKeyProvider : IIdentityKeyProvider
    {
        public string Name => "DefaultHash";

        public IEqualityComparer<IKey> GetComparer() => new Key32();

        public IKey GetKey(Memory<byte> keyBytes)
        {
            if (keyBytes.Length != 32)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 32 bytes");
            }

            return new Key32(keyBytes);
        }
    }
}

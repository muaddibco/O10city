using System;
using System.Collections.Generic;
using O10.Core.Architecture;

namespace O10.Core.Identity
{
    [ExtensionPoint]
    public interface IIdentityKeyProvider
    {
        string Name { get; }

        //IKey GetKey(byte[] keyBytes);

        IKey? GetKey(Memory<byte> keyBytes);

        IEqualityComparer<IKey> GetComparer();
    }
}

using System;

namespace O10.Core.Communication
{
    public interface IPacketProvider : IDisposable
    {
        byte[] GetBytes();
    }
}

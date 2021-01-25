using System.Net.Sockets;
using O10.Core.Architecture;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface IBufferManager
    {
        int BufferSize { get; }

        void InitBuffer(int totalBytes, int bufferSize);
        bool SetBuffer(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs);
        void FreeBuffer(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs);
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using O10.Core.Architecture;
using O10.Network.Communication;
using O10.Network.Handlers;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface ICommunicationChannel : IDisposable
    {
        event EventHandler<EventArgs> SocketClosedEvent;

        IPAddress RemoteIPAddress { get; }

        void PushForParsing(byte[] buf, int offset, int count);

        void Init(IBufferManager bufferManager, IPacketsHandler packetsHandler);

        Task<OperationStatus<ICommunicationChannel>> StartConnectionAsync(Socket socket, EndPoint endPoint);

        void RegisterExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceivedExtendedValidation);

        void Close();

        void AcceptSocket(Socket acceptSocket);

        void PostMessage(byte[] message);
    }
}

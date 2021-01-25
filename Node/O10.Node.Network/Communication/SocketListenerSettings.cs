using System.Net;

namespace O10.Network.Communication
{
    public class SocketListenerSettings : SocketSettings
    {
        /// <summary>
        /// Endpoint for the listener
        /// </summary>
        public IPEndPoint ListeningEndpoint { get; }

        public SocketListenerSettings(int maxConnections, int receiveBufferSize, IPEndPoint listeningEndpoint) 
            : base(maxConnections, receiveBufferSize, listeningEndpoint.Port, listeningEndpoint.AddressFamily)
        {
            ListeningEndpoint = listeningEndpoint;
        }
    }
}

﻿using System;
using System.Net;
using O10.Network.Properties;

namespace O10.Network.Exceptions
{

    [Serializable]
    public class SocketIsDisconnectedException : Exception
    {
        public SocketIsDisconnectedException() { }
        public SocketIsDisconnectedException(IPEndPoint ipEndPoint) : base(string.Format(Resources.ERR_SOCKET_IS_DISCONNECTED, ipEndPoint.Address, ipEndPoint.Port)) { }
        public SocketIsDisconnectedException(IPEndPoint ipEndPoint, Exception inner) : base(string.Format(Resources.ERR_SOCKET_IS_DISCONNECTED, ipEndPoint.Address, ipEndPoint.Port), inner) { }
        protected SocketIsDisconnectedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

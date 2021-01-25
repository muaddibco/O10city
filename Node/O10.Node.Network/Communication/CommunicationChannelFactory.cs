using O10.Network.Interfaces;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Network.Communication
{
    public class CommunicationChannelFactory : ICommunicationChannelFactory
    {
        private readonly Stack<ICommunicationChannel> _handlers;
        private readonly IServiceProvider _serviceProvider;

        public CommunicationChannelFactory(IServiceProvider serviceProvider)
        {
            _handlers = new Stack<ICommunicationChannel>();
            _serviceProvider = serviceProvider;
        }

        public ICommunicationChannel Create()
        {
            if(_handlers.Count > 1)
            {
                return _handlers.Pop();
            }

            return _serviceProvider.GetService<ICommunicationChannel>();
        }

        public void Utilize(ICommunicationChannel handler)
        {
            _handlers.Push(handler);
        }
    }
}

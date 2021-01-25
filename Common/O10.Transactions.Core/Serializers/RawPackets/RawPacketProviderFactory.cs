using System.Collections.Generic;
using O10.Core.Architecture;

using O10.Core.Models;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Transactions.Core.Serializers.RawPackets
{
    [RegisterDefaultImplementation(typeof(IRawPacketProvidersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class RawPacketProvidersFactory : IRawPacketProvidersFactory
    {
        private readonly Stack<IRawPacketProvider> _rawPacketProviders;
        private readonly IServiceProvider _serviceProvider;

        public RawPacketProvidersFactory(IServiceProvider serviceProvider)
        {
            _rawPacketProviders = new Stack<IRawPacketProvider>();
            _serviceProvider = serviceProvider;
        }

        public IRawPacketProvider Create()
        {
            if(_rawPacketProviders.Count > 0)
            {
                return _rawPacketProviders.Pop();
            }
            else
            {
                IRawPacketProvider rawPacketProvider = _serviceProvider.GetService<IRawPacketProvider>();

                return rawPacketProvider;
            }
        }

        public IRawPacketProvider Create(IPacket blockBase)
        {
            IRawPacketProvider rawPacketProvider = Create();

            rawPacketProvider.Initialize(blockBase);

            return rawPacketProvider;
        }

		public IRawPacketProvider Create(byte[] content)
		{
			IRawPacketProvider rawPacketProvider = Create();

			rawPacketProvider.Initialize(content);

			return rawPacketProvider;
		}

		public void Utilize(IRawPacketProvider obj)
        {
            _rawPacketProviders.Push(obj);
        }
    }
}

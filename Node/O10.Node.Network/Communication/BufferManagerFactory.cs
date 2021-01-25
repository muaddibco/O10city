using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using O10.Network.Interfaces;
using O10.Core.Architecture;

namespace O10.Network.Communication
{
    [RegisterDefaultImplementation(typeof(IBufferManagerFactory), Lifetime = LifetimeManagement.Singleton)]
    public class BufferManagerFactory : IBufferManagerFactory
    {
        private readonly Stack<IBufferManager> _bufferManagersPool = new Stack<IBufferManager>();
        private readonly IServiceProvider _serviceProvider;

        public BufferManagerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBufferManager Create()
        {
            if (_bufferManagersPool.Count > 0)
            {
                lock (_bufferManagersPool)
                {
                    if (_bufferManagersPool.Count > 0)
                    {
                        return _bufferManagersPool.Pop();
                    }
                    else
                    {
                        
                        return _serviceProvider.GetService<IBufferManager>();
                    }
                }
            }
            else
            {
                return _serviceProvider.GetService<IBufferManager>();
            }
        }

        public void Utilize(IBufferManager bufferManager)
        {
            if (bufferManager == null)
            {
                throw new ArgumentNullException(nameof(bufferManager));
            }

            _bufferManagersPool.Push(bufferManager);
        }
    }

}

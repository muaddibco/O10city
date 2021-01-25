using O10.Network.Interfaces;
using O10.Network.Communication;
using System.Collections.Generic;
using O10.Network.Handlers;

namespace O10.Network.Tests.Fixtures
{
    public class DependencyInjectionFixture
    {
        public DependencyInjectionFixture()
        {
            BufferManager = new BufferManager();
            BufferManager.InitBuffer(200, 100);
        }

        public IPacketsHandler PacketsHandler { get; }

        public List<byte[]> Packets { get; }

        public IBufferManager BufferManager { get; set; }
    }
}

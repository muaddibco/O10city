using O10.Network.Interfaces;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using O10.Network.Tests.Fixtures;
using Xunit;
using O10.Core.Logging;
using O10.Network.Communication;
using O10.Core;
using O10.Network.Handlers;

namespace O10.Network.Tests
{
    public class ClientHandlerTests : IClassFixture<DependencyInjectionFixture>
    {
        public ClientHandlerTests(DependencyInjectionFixture dependencyInjectionFixture)
        {
            DependencyInjectionFixture = dependencyInjectionFixture;
        }

        public DependencyInjectionFixture DependencyInjectionFixture { get; }

        [Fact]
        public void ParseSingleShortPacket()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel(loggerService, null);
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet = new byte[] { Globals.DLE, Globals.STX, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44};
            byte[] expectedPacket = new byte[] { 0xaa, 0xbb, 0xcc};

            handler.PushForParsing(packet, 0, packet.Length);

            Thread.Sleep(100);

            handler.Close();

            Assert.True(packets.Count == 1);

            byte[] actualPacket = packets.First();
            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void ParseSingleShortExactPacket()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel(loggerService, null);
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet = new byte[] { Globals.DLE, Globals.STX, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xaa, 0xbb, 0xcc};
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc };

            handler.PushForParsing(packet, 0, packet.Length);

            Thread.Sleep(100);

            handler.Close();

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleShortPacketWithDLE()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel(loggerService, null);
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet = new byte[] { Globals.DLE, Globals.STX, Globals.DLE, Globals.DLE + 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xaa, 0xbb, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb };

            handler.PushForParsing(packet, 0, packet.Length);

            Thread.Sleep(100);

            handler.Close();

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleLongPacket()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel(loggerService, null);
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet1 = new byte[] { Globals.DLE, Globals.STX, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] packet2 = new byte[] { 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x44, 0x03, 0x00, 0xaa, 0xbb };

            handler.PushForParsing(packet1, 0, packet1.Length);
            handler.PushForParsing(packet2, 0, packet2.Length);

            Thread.Sleep(100);

            handler.Close();

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleLongPacketDleIsLast()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel(loggerService, null);
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet1 = new byte[] { 0x45, 0x65, Globals.DLE };
            byte[] packet2 = new byte[] { Globals.STX, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] packet3 = new byte[] { 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x44, 0x03, 0x00, 0xaa, 0xbb };

            handler.PushForParsing(packet1, 0, packet1.Length);
            handler.PushForParsing(packet2, 0, packet2.Length);
            handler.PushForParsing(packet3, 0, packet3.Length);

            Thread.Sleep(100);

            handler.Close();

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }
    }
}

using O10.Network.Interfaces;
using O10.Network.Communication;
using NSubstitute;
using System.Net;
using System.Net.Sockets;
using O10.Network.Tests.Fixtures;
using Xunit;
using O10.Core.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Network.Tests
{
    public class CommunicationHubTests : IClassFixture<DependencyInjectionFixture>
    {
        [Theory]
        [InlineData(3001)]
        public void ConnectivityBaseTest(int listeningPort)
        {
            IServiceCollection services = new ServiceCollection();

            ICommunicationChannel clientHandler = Substitute.For<ICommunicationChannel>();

            services.AddSingleton(typeof(ICommunicationChannel), clientHandler);
            var serviceProvider = services.BuildServiceProvider();

            ILoggerService loggerService = Substitute.For<ILoggerService>();

            IBufferManagerFactory bufferManagerFactory = Substitute.For<IBufferManagerFactory>();
            ServerCommunicationServiceBase communicationHub = new TcpCommunicationService(serviceProvider, loggerService, bufferManagerFactory, null, null);

            IPEndPoint communicationEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listeningPort);
            SocketListenerSettings settings = new SocketListenerSettings(1, 100, communicationEndPoint);

            communicationHub.Init(settings);
            communicationHub.Start();

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(communicationEndPoint.Address, communicationEndPoint.Port);

            Assert.True(tcpClient.Connected);
        }

        //TODO: 
        // CommunicationProvisioning tests
    }
}

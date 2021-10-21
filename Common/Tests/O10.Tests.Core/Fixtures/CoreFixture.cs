using O10.Core.Logging;
using NSubstitute;
using Xunit.Abstractions;
using O10.Core.Identity;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Configuration;
using System;

namespace O10.Tests.Core.Fixtures
{
    public class CoreFixture
    {
        private ITestOutputHelper _testOutputHelper;

        public CoreFixture()
        {
            ServiceCollection = new ServiceCollection();

            LoggerService = Substitute.For<ILoggerService>();
            IdentityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IdentityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
            ConfigurationService = Substitute.For<IConfigurationService>();

            ServiceCollection
                .AddSingleton(LoggerService)
                .AddSingleton(ConfigurationService)
                .AddSingleton(IdentityKeyProvidersRegistry);
        }

        public void InjectOutputHelper(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            LoggerService.GetLogger(null).ReturnsForAnyArgs(new TestLogger(_testOutputHelper));
        }

        public IServiceProvider BuildContainer()
        {
            ServiceProvider = ServiceCollection.BuildServiceProvider();

            return ServiceProvider;
        }

        public IConfigurationService ConfigurationService { get; }

        public ILoggerService LoggerService { get; set; }

        public IIdentityKeyProvidersRegistry IdentityKeyProvidersRegistry { get; }

        public IServiceCollection ServiceCollection { get; }
        public IServiceProvider ServiceProvider { get; private set; }
    }
}

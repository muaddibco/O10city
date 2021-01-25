using O10.Core.Logging;
using NSubstitute;
using Xunit.Abstractions;

namespace O10.Tests.Core.Fixtures
{
    public class CoreFixture
    {
        private ITestOutputHelper _testOutputHelper;

        public CoreFixture()
        {
            LoggerService = Substitute.For<ILoggerService>();
        }

        public void InjectOutputHelper(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            LoggerService.GetLogger(null).ReturnsForAnyArgs(new TestLogger(_testOutputHelper));
        }

        public ILoggerService LoggerService { get; set; }
    }
}

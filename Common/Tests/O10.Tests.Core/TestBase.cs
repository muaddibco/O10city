using O10.Tests.Core.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace O10.Tests.Core
{
    public abstract class TestBase : IClassFixture<CoreFixture>
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestBase(CoreFixture coreFixture, ITestOutputHelper testOutputHelper)
        {
            CoreFixture = coreFixture ?? throw new System.ArgumentNullException(nameof(coreFixture));
            _testOutputHelper = testOutputHelper;
            coreFixture.InjectOutputHelper(_testOutputHelper);
        }

        protected CoreFixture CoreFixture { get; }
    }
}

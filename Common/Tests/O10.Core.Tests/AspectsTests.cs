using NSubstitute;
using Xunit;
using O10.Core.Configuration;
using O10.Core.Tests.Classes;

namespace O10.Core.Tests
{
    public class AspectsTests 
    {
        public AspectsTests()
        {
        }

        [Fact]
        public void ConfigurationSectionTest()
        {
            IAppConfig appConfig = Substitute.For<IAppConfig>();
            appConfig.GetString(null).ReturnsForAnyArgs(ci =>
            {
                switch (ci.Arg<string>().ToLower())
                {
                    case "configa:maxvalue":
                        return "10";
                    case "configb:maxvalue":
                        return "20";
                }

                return null;
            });

            ConfigurationService configurationService = new ConfigurationService(new IConfigurationSection[2] { new ConfigA(appConfig), new ConfigB(appConfig) });
            
            ConfigA configA = (ConfigA)configurationService["configA"];
            ConfigB configB = (ConfigB)configurationService["configB"];
            configA.Initialize();
            configB.Initialize();

            ushort maxValueA = configA.MaxValue;
            ushort maxValueB = configB.MaxValue;

            Assert.Equal(10, maxValueA);
            Assert.Equal(20, maxValueB);
        }

        [Fact]
        public void ConfigurationSectionArrayValueTest()
        {
            IAppConfig appConfig = Substitute.For<IAppConfig>();
            appConfig.GetString(null).ReturnsForAnyArgs(ci =>
            {
                return (ci.Arg<string>().ToLower()) switch
                {
                    "configroles:roles" => "roleA, roleB",
                    _ => null,
                };
            });

            ConfigurationService configurationService = new ConfigurationService(new IConfigurationSection[1] { new ConfigRoles(appConfig) });

            ConfigRoles configRoles = (ConfigRoles)configurationService["configroles"];
            configRoles.Initialize();
            Assert.Equal(2, configRoles.Roles.Length);
            Assert.Contains("roleA", configRoles.Roles);
            Assert.Contains("roleB", configRoles.Roles);
        }

        [Fact]
        public void ConfigurationSectionIntArrayValueTest()
        {
            IAppConfig appConfig = Substitute.For<IAppConfig>();
            appConfig.GetString(null).ReturnsForAnyArgs(ci =>
            {
                return (ci.Arg<string>().ToLower()) switch
                {
                    "configints:ints" => "5, 10",
                    _ => null,
                };
            });

            ConfigurationService configurationService = new ConfigurationService(new IConfigurationSection[1] { new ConfigInts(appConfig) });

            ConfigInts configInts = (ConfigInts)configurationService["configints"];
            configInts.Initialize();

            Assert.Equal(2, configInts.Ints.Length);
            Assert.Contains(5, configInts.Ints);
            Assert.Contains(10, configInts.Ints);
        }
    }
}

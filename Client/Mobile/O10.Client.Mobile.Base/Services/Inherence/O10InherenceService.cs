using Flurl;
using Flurl.Http;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    [RegisterDefaultImplementation(typeof(IO10InherenceService), Lifetime = LifetimeManagement.Singleton)]
    public class O10InherenceService : IO10InherenceService
    {
        private readonly IO10InherenceConfiguration _o10InherenceConfiguration;

        public O10InherenceService(IConfigurationService configurationService)
        {
            _o10InherenceConfiguration = configurationService.Get<IO10InherenceConfiguration>();
        }

        public Task<HttpResponseMessage> RequestO10InherenceServer(byte[] sessionKey, (byte[] commitment, byte[] image)[] commitmentImages)
        {
            BiometricPersonDataDto biometricPersonData = new BiometricPersonDataDto
            {
                SessionKey = sessionKey.ToHexString(),
                Images = commitmentImages.ToDictionary(c => c.commitment.ToHexString(), c => c.image)
            };

            return _o10InherenceConfiguration.Uri.AppendPathSegment("RegisterPerson").PostJsonAsync(biometricPersonData);
        }
    }
}

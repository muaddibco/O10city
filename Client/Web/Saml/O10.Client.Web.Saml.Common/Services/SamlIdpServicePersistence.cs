using O10.Client.Common.Communication;
using O10.Client.Common.Interfaces;

namespace O10.Client.Web.Saml.Common.Services
{
    public class SamlIdpServicePersistence
	{
		public IWitnessPackagesProvider WitnessPackagesProvider { get; set; }
		public IStealthClientCryptoService ClientCryptoService { get; set; }
		public StealthPacketsExtractor PacketsExtractor { get; set; }
		public SamlIdpService SamlIdpService { get; set; }
		public SamlIdpWitnessPackageUpdater WitnessPackageUpdater { get; set; }
	}
}

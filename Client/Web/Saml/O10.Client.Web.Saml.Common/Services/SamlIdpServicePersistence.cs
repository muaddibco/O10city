using O10.Client.Common.Interfaces;
using O10.Client.Stealth;
using O10.Client.Stealth.Ingress;

namespace O10.Client.Web.Saml.Common.Services
{
    public class SamlIdpServicePersistence
	{
		public IWitnessPackagesProvider WitnessPackagesProvider { get; set; }
		public IStealthClientCryptoService ClientCryptoService { get; set; }
		public PacketsExtractor PacketsExtractor { get; set; }
		public SamlIdpService SamlIdpService { get; set; }
		public SamlIdpWitnessPackageUpdater WitnessPackageUpdater { get; set; }
	}
}

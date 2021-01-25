using Saml2.Authentication.Core.Bindings;

namespace O10.Client.Web.Saml.Common.Dtos
{
	public class SamlIdpSessionResponse
	{
		public string SessionId { get; set; }

		public string RedirectUri { get; set; }

		public Saml2Response Saml2Response { get; set; }

		public string Signature { get; set; }

		public string SignatureAlgorithm { get; set; }
	}
}

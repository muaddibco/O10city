using System;
using O10.Client.Web.Common.Dtos.SamlIdp;

namespace O10.Client.Web.Saml.Common.Dtos
{
	public class SamlIdpSessionPersistence
	{
        public DateTime CreationTime { get; set; }
        public string SessionKey { get; set; }
		public string InResponseTo { get; set; }
		public string RedirectUri { get; set; }
        public string SingleLogoutUri { get; set; }
        public string RelayState { get; set; }

		public SamlIdpSessionInfo SessionInfo { get; set; }
	}
}

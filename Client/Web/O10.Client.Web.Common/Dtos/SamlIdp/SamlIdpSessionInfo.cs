using System.Collections.Generic;

namespace O10.Client.Web.Common.Dtos.SamlIdp
{
	public class SamlIdpSessionInfo
	{
		public string SessionKey { get; set; }

		public string TargetPublicSpendKey { get; set; }

        public string TargetPublicViewKey { get; set; }

        public List<string> Validations { get; set; }
    }
}

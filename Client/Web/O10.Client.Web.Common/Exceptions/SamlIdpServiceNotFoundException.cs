using System;
using O10.Client.Web.Common.Properties;

namespace O10.Client.Web.Common.Exceptions
{

    [Serializable]
	public class SamlIdpServiceNotFoundException : Exception
	{
		public SamlIdpServiceNotFoundException() { }
		public SamlIdpServiceNotFoundException(string entityId) : base(string.Format(Resources.ERR_SAML_IDP_SERVICE_NOT_FOUND, entityId)) { }
		public SamlIdpServiceNotFoundException(string entityId, Exception inner) : base(string.Format(Resources.ERR_SAML_IDP_SERVICE_NOT_FOUND, entityId), inner) { }
		protected SamlIdpServiceNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}

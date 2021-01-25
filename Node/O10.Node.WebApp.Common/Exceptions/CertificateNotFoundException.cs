using System;
using O10.Node.WebApp.Common.Properties;

namespace O10.Node.WebApp.Common.Exceptions
{

	[Serializable]
	public class CertificateNotFoundException : Exception
	{
		public CertificateNotFoundException() { }
		public CertificateNotFoundException(string thumbprint, string storage) : base(string.Format(Resources.ERR_CERT_NOT_FOUND, thumbprint, storage)) { }
		public CertificateNotFoundException(string thumbprint, string storage, Exception inner) : base(string.Format(Resources.ERR_CERT_NOT_FOUND, thumbprint, storage), inner) { }
		protected CertificateNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}

using System.Threading.Tasks;
using O10.Client.Common.Interfaces.Outputs;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
	[ServiceContract]
	public interface IDocumentSignatureVerifier
	{
		Task<DocumentSignatureVerification> Verify(byte[] documentCreator, byte[] documentHash, ulong documentRecordHeight, ulong signatureRecordBlockHeight);
	}
}

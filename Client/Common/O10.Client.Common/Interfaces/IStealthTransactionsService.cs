using O10.Core.Architecture;
using System.Threading.Tasks;
using O10.Client.Common.Identities;
using O10.Client.Common.Interfaces.Inputs;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Transactions.Core.DTOs;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
	public interface IStealthTransactionsService : ITransactionsService
	{
		void Initialize(long accountId);

        Task<RequestResult> SendRelationsProofs(RelationsProofsInput relationsProofsInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments);

		Task<RequestResult> SendDocumentSignRequest(DocumentSignRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments);

		Task<RequestResult> SendEmployeeRegistrationRequest(EmployeeRequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[][] issuanceCommitments);

        Task<RequestResult> SendIdentityProofs(RequestInput requestInput, AssociatedProofPreparation[] associatedProofPreparations, OutputModel[] outputModels, byte[] issuer);
        Task<RequestResult> SendRevokeIdentity(RequestInput requestInput, OutputModel[] outputModels, byte[][] issuanceCommitments);

		Task<RequestResult> SendCompromisedProofs(RequestInput requestInput, byte[] compromisedKeyImage, byte[] compromisedTransactionKey, byte[] destinationKey, OutputModel[] outputModels, byte[][] issuanceCommitments);
		Task<RequestResult> SendUniversalTransport(RequestInput requestInput, OutputModel[] outputModels, UniversalProofs universalProofs);

		ISourceBlock<byte[]> PipeOutKeyImages { get; }
	}
}

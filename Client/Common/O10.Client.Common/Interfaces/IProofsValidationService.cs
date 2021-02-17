using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Cryptography;
using O10.Core.Architecture;
using O10.Client.Common.Entities;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IProofsValidationService
    {
        Task<bool> CheckSpIdentityValidations(Memory<byte> commitment, AssociatedProofs[] associatedProofsList, IEnumerable<ValidationCriteria> validationCriterias, string issuer);
        Task<bool> CheckAssociatedProofs(RootIssuer rootIssuer, IEnumerable<ValidationCriteria> validationCriterias);
        Task CheckEligibilityProofs(Memory<byte> assetCommitment, SurjectionProof eligibilityProofs, Memory<byte> issuer);
        Task VerifyKnowledgeFactorProofs(List<RootIssuer> rootIssuers);
        (long registrationId, bool isNew) HandleRegistration(long accountId, Memory<byte> assetCommitment, SurjectionProof authenticationProof);
    }
}

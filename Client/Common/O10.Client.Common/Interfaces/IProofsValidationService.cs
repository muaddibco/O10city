using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Architecture;
using O10.Crypto.Models;
using O10.Client.Common.Dtos;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IProofsValidationService
    {
        Task<bool> CheckAssociatedProofs(RootIssuer rootIssuer, IEnumerable<ValidationCriteriaDTO> validationCriterias);
        Task CheckEligibilityProofs(Memory<byte> assetCommitment, SurjectionProof eligibilityProofs, Memory<byte> issuer);
        Task VerifyKnowledgeFactorProofs(List<RootIssuer> rootIssuers);
        (long registrationId, bool isNew) HandleRegistration(long accountId, Memory<byte> assetCommitment, SurjectionProof authenticationProof);
    }
}

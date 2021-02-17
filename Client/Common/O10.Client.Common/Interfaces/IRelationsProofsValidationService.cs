using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces.Inputs;
using O10.Core.Architecture;
using System;

namespace O10.Client.Common.Interfaces
{
    [Obsolete("This interface will be replaced with IProofsValidationService")]
    [ServiceContract]
    public interface IRelationsProofsValidationService
    {
        Task<RelationProofsValidationResults> VerifyRelationProofs(GroupsRelationsProofs relationsProofs, IStealthClientCryptoService clientCryptoService, RelationProofsSession relationProofsSession);
    }
}

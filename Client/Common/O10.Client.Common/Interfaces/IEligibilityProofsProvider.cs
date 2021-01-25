using O10.Core.Architecture;
using O10.Core.Cryptography;
using System;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IEligibilityProofsProvider
    {
        Task<SurjectionProof> CreateEligibilityProof(byte[] originalCommitment, byte[] originalBlindingFactor, byte[] assetCommitment, byte[] newBlindingFactor, Memory<byte> issuer);

        void GetEligibilityCommitmentAndProofs(byte[] ownedCommitment, byte[][] inputCommitments, out int actualAssetPos, out byte[][] outputCommitments);
    }
}

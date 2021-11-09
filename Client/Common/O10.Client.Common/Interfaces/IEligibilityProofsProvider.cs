using O10.Core.Architecture;
using O10.Crypto.Models;
using System;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IEligibilityProofsProvider
    {
        Task<SurjectionProof> CreateEligibilityProof(Memory<byte> assetCommitment,
                                                     Memory<byte> newBlindingFactor,
                                                     byte[] originalCommitment,
                                                     byte[] originalBlindingFactor,
                                                     Memory<byte> issuer);

        void GetEligibilityCommitments(byte[] ownedCommitment, byte[][] inputCommitments, out int actualAssetPos, out byte[][] outputCommitments);
    }
}

using O10.Client.Common.Dtos.UniversalProofs;
using O10.Crypto.ConfidentialAssets;
using System;
using System.Threading.Tasks;

namespace O10.Client.Common.Interfaces
{
    public class RelationsCreationPayloadFactory
    {
        private readonly IBoundedAssetsService _boundedAssetsService;

        public RelationsCreationPayloadFactory(IBoundedAssetsService boundedAssetsService)
        {
            _boundedAssetsService = boundedAssetsService;
        }

        public async Task<RelationsCreationPayload> CreateRelationsCreation(Memory<byte> rootBf)
        {
            RelationsCreationPayload relationsCreationPayload = new RelationsCreationPayload();
            byte[] localBf = CryptoHelper.GetRandomSeed();
            byte[] localToRootDiff = CryptoHelper.GetDifferentialBlindingFactor(localBf, rootBf.Span);

            throw new NotImplementedException();
        }
    }
}

using Newtonsoft.Json;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;
using Xunit;

namespace O10.Client.Common.Tests.UniversalProofsTests
{
    public class SerializeDeserializeUniversalProofTests
    {
        [Fact]
        public void SerializeUniversalProofTest()
        {
            byte[] issuer = CryptoHelper.GetRandomSeed();
            byte[] commitment = CryptoHelper.GetRandomSeed();

            UniversalProofs universalProofs = new UniversalProofs
            {
                MainIssuer = new Key32(issuer),
                KeyImage = new Key32(commitment)
            };

            string json = JsonConvert.SerializeObject(universalProofs);
            Assert.True(json.Contains($"\"MainIssuer\":\"{issuer.ToHexString()}\""));
            Assert.True(json.Contains($"\"KeyImage\":\"{commitment.ToHexString()}\""));
        }

        [Fact]
        public void DeserialzeUniversalProofTest()
        {
            const string json = "{\"MainIssuer\":\"5905D9ECFD5750828319D1BDBB5B6ADF0432065EFE94463E1635B78EB4333002\",\"KeyImage\":\"A1629AAF19F9FBEB3C0F82B3286D56EFC2CA34CC843E08C22ECD4BBEB87ED701\"}";

            UniversalProofs universalProofs = (UniversalProofs)JsonConvert.DeserializeObject(json, typeof(UniversalProofs));

            Assert.True("5905D9ECFD5750828319D1BDBB5B6ADF0432065EFE94463E1635B78EB4333002".HexStringToByteArray().Equals32(universalProofs.MainIssuer.ArraySegment.Array));
            Assert.True("A1629AAF19F9FBEB3C0F82B3286D56EFC2CA34CC843E08C22ECD4BBEB87ED701".HexStringToByteArray().Equals32(universalProofs.KeyImage.ArraySegment.Array));
        }
    }
}

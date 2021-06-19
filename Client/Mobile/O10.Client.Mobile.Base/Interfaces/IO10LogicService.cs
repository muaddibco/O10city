using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces.Inputs;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IO10LogicService
    {
        Task SendUniversalTransport([NotNull] RequestInput requestInput, [NotNull] UniversalProofs universalProofs, [NotNull] string serviceProviderInfo, bool storeRegistration = false);

        Task<bool> StoreRegistration(byte[] targetPublicSpendKey, string spInfo, Memory<byte> issuer, params Memory<byte>[] assetIds);

        Task StoreAssociatedAttributes(string rootIssuer, byte[] rootAssetId, string associatedIssuer, IEnumerable<AttributeValue> attributeValues);
    }
}

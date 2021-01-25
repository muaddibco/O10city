﻿using System;
using System.Collections.Generic;
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
        Task SendIdentityProofs(RequestInput requestInput);
        Task SendUniversalTransport(RequestInput requestInput, UniversalProofs universalProofs, string serviceProviderInfo, bool storeRegistration = false);

        Task<bool> StoreRegistration(byte[] targetPublicSpendKey, string spInfo, Memory<byte> issuer, params Memory<byte>[] assetIds);

        Task StoreAssociatedAttributes(string rootIssuer, byte[] rootAssetId, string associatedIssuer, IEnumerable<AttributeValue> attributeValues);
    }
}

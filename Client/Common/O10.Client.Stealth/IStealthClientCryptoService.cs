﻿using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Identity;
using System;

namespace O10.Client.Stealth
{
    [ServiceContract]
    public interface IStealthClientCryptoService : IClientCryptoService
    {
        byte[] GetKeyImage(IKey transactionPublicKey);
        byte[] GetBlindingFactor(IKey transactionPublicKey);
        byte[] GetBlindingFactor(Memory<byte> transactionPublicKey);
    }
}

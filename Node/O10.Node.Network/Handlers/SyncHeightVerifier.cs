using System;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.HashCalculations;
using O10.Core.States;
using O10.Core;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Network.Synchronization;

namespace O10.Network.Handlers
{
    [RegisterExtension(typeof(ICoreVerifier), Lifetime = LifetimeManagement.Transient)]
    public class SyncHeightVerifier : ICoreVerifier
    {
        private readonly ILogger _log;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _proofOfWorkCalculation;

        public SyncHeightVerifier(IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _proofOfWorkCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
        }

        public bool VerifyBlock(IPacketBase packet)
        {
            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Transaction is RegistryTransactionBase transaction)
            {
                long syncBlockHeight = packet.AsPacket<RegistryPacket>().Payload.SyncHeight;

                bool isInSyncRange = _synchronizationContext.LastBlockDescriptor == null 
                    || (_synchronizationContext.LastBlockDescriptor.BlockHeight.Equals(syncBlockHeight) 
                    || (_synchronizationContext.LastBlockDescriptor.BlockHeight - 1).Equals(syncBlockHeight) 
                    || (_synchronizationContext.LastBlockDescriptor.BlockHeight - 2).Equals(syncBlockHeight));

                if (!isInSyncRange)
                {
                    _log.Error($"Synchronization block height ({syncBlockHeight}) is outdated [{packet.GetType().Name}]: {packet}");
                    return false;
                }
            }

            return true;
        }

        //private bool CheckSyncPOW(IPacketBase packet)
        //{
        //    ulong syncBlockHeight = packet.SyncHeight;

        //    uint nonce = packet.Nonce;
        //    byte[] powHash = packet.PowHash;
        //    byte[] baseHash;
        //    byte[] baseSyncHash;

        //    if (packet.LedgerType != (ushort)LedgerType.Synchronization)
        //    {
        //        //TODO: make difficulty check dynamic
        //        //if (powHash[0] != 0 || powHash[1] != 0)
        //        //{
        //        //    return false;
        //        //}
        //        BigInteger bigInteger;
        //        baseSyncHash = new byte[Globals.DEFAULT_HASH_SIZE + 1]; // Adding extra 0 byte for avoiding negative values of BigInteger
        //        lock (_synchronizationContext)
        //        {
        //            byte[] buf;
        //            if (_synchronizationContext.LastBlockDescriptor != null || _synchronizationContext.PrevBlockDescriptor != null)
        //            {
        //                buf = (syncBlockHeight == _synchronizationContext.LastBlockDescriptor?.BlockHeight) ? _synchronizationContext.LastBlockDescriptor.Hash : _synchronizationContext.PrevBlockDescriptor.Hash;
        //            }
        //            else
        //            {
        //                _log.Warning("CheckSyncPOW - BOTH LastBlockDescriptor and PrevBlockDescriptor are NULL");
        //                buf = new byte[Globals.DEFAULT_HASH_SIZE];
        //            }

        //            Array.Copy(buf, 0, baseSyncHash, 0, buf.Length);
        //        }

        //        bigInteger = new BigInteger(baseSyncHash);

        //        bigInteger += nonce;
        //        baseHash = bigInteger.ToByteArray().Take(Globals.DEFAULT_HASH_SIZE).ToArray();
        //    }
        //    else
        //    {
        //        lock (_synchronizationContext)
        //        {
        //            if (_synchronizationContext.LastBlockDescriptor == null)
        //            {
        //                baseSyncHash = new byte[Globals.DEFAULT_HASH_SIZE];
        //            }
        //            else
        //            {
        //                baseSyncHash = (syncBlockHeight == _synchronizationContext.LastBlockDescriptor.BlockHeight) ? _synchronizationContext.LastBlockDescriptor.Hash : _synchronizationContext.PrevBlockDescriptor.Hash;
        //            }
        //        }

        //        baseHash = baseSyncHash;
        //    }

        //    byte[] computedHash = _proofOfWorkCalculation.CalculateHash(baseHash);

        //    if (!computedHash.Equals24(powHash))
        //    {
        //        _log.Error($"Computed HASH differs from obtained one. PacketType is {packet.LedgerType}, BlockType is {packet.PacketType}. Reported SyncBlockHeight is {packet.SyncHeight}, Nonce is {packet.Nonce}, POW is {packet.PowHash.ToHexString()}. Hash of SyncBlock is {baseSyncHash.ToHexString()}, after adding Nonce is {baseHash.ToHexString()}, computed POW Hash is {computedHash.ToHexString()}");
        //        return false;
        //    }

        //    return true;
        //}
    }
}

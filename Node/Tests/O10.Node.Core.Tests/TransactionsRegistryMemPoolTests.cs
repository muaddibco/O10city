﻿using Chaos.NaCl;
using HashLib;
using NSubstitute;
using System;
using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Serializers.Signed.Registry;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.States;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto.HashCalculations;
using O10.Node.Core.Registry;
using Xunit;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers;
using O10.Network.Synchronization;

namespace O10.Node.Core.Tests
{
    public class TransactionsRegistryMemPoolTests
    {

        [Fact]
        public void MemPool_AddedNonUniqueTransactions_NotAllContained()
        {
            SynchronizationDescriptor synchronizationDescriptor = new SynchronizationDescriptor(1, new byte[Globals.DEFAULT_HASH_SIZE], DateTime.Now, DateTime.Now, 1);
            IHash transactionKeyHash = HashFactory.Hash128.CreateMurmur3_128();
            ILogger logger = Substitute.For<ILogger>();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            IIdentityKeyProvider identityKeyProvider = new TransactionRegistryKeyProvider();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
            ISigningService signingService = Substitute.For<ISigningService>();
            SynchronizationContext synchronizationContext = new SynchronizationContext(loggerService);
            synchronizationContext.UpdateLastSyncBlockDescriptor(synchronizationDescriptor);
            IStatesRepository statesRepository = Substitute.For<IStatesRepository>();

            hashCalculationsRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());

            logger.WhenForAnyArgs(l => l.Error(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Warning(null)).DoNotCallBase();
            logger.WhenForAnyArgs(l => l.Info(null)).DoNotCallBase();
            loggerService.GetLogger(null).Returns(logger);

            identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().ReturnsForAnyArgs(identityKeyProvider);
            //identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key16() { Value = c.Arg<Memory<byte>>() });

            statesRepository.GetInstance<ISynchronizationContext>().Returns(synchronizationContext);

            byte[] privateKey = CryptoHelper.GetRandomSeed();
            Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] expandedPrivateKey, privateKey);

            signingService.WhenForAnyArgs(s => s.Sign(null, null)).Do(c =>
            {
                ((OrderedPacketBase)c.ArgAt<IPacket>(0)).Source = new Key32(publicKey);
                ((OrderedPacketBase)c.ArgAt<IPacket>(0)).Signature = Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey);
            });
            signingService.PublicKeys.ReturnsForAnyArgs(new IKey[] { new Key32(publicKey) });

            TransactionRegistryMemPool transactionRegistryMemPool = new TransactionRegistryMemPool(loggerService, identityKeyProvidersRegistry, statesRepository, hashCalculationsRepository);

            SortedList<ushort, RegistryRegisterBlock> expectedBlocks = new SortedList<ushort, RegistryRegisterBlock>();

            ulong[] heights = new ulong[] { 1, 2, 2, 5, 4, 3, 4, 3, 4, 5, 3};

            HashSet<ulong> addedHeights = new HashSet<ulong>();
            ushort order = 0;

            for (ulong i = 0; i < (ulong)heights.Length; i++)
            {
                RegistryRegisterBlock transactionRegisterBlock = PacketsBuilder.GetTransactionRegisterBlock(synchronizationContext.LastBlockDescriptor.BlockHeight, 1, null, 
                    heights[i],LedgerType.O10State, TransactionTypes.Transaction_IssueBlindedAsset, new byte[Globals.POW_HASH_SIZE], new byte[Globals.DEFAULT_HASH_SIZE],privateKey);

                RegistryRegisterBlockSerializer serializer = new RegistryRegisterBlockSerializer(null);
                serializer.Initialize(transactionRegisterBlock);
                serializer.SerializeBody();
                signingService.Sign(transactionRegisterBlock);
                serializer.SerializeFully();

                if (!addedHeights.Contains(heights[i]))
                {
                    expectedBlocks.Add(order++, transactionRegisterBlock);
                    addedHeights.Add(heights[i]);
                }

                transactionRegistryMemPool.EnqueueTransactionWitness(transactionRegisterBlock);
            }

            SortedList<ushort, RegistryRegisterBlock> actualBlocks = transactionRegistryMemPool.DequeueStateWitnessBulk();

            Assert.Equal(expectedBlocks.Count, actualBlocks.Count);
            for (ushort i = 0; i < (ushort)expectedBlocks.Count; i++)
            {
                Assert.Equal(expectedBlocks[i].Height, actualBlocks[i].Height);
            }
        }

        private static long GetConfidence(byte[] bitMask)
        {
            long sum = 0;
            byte[] numBytes = new byte[8];
            for (int i = 0; i < bitMask.Length; i += 8)
            {
                long num;
                if (bitMask.Length - i < 8)
                {
                    numBytes[0] = 0;
                    numBytes[1] = 0;
                    numBytes[2] = 0;
                    numBytes[3] = 0;
                    numBytes[4] = 0;
                    numBytes[5] = 0;
                    numBytes[6] = 0;
                    numBytes[7] = 0;

                    Array.Copy(bitMask, i, numBytes, 0, bitMask.Length - i);
                    num = BitConverter.ToInt64(numBytes, 0);
                }
                else
                {
                    num = BitConverter.ToInt64(bitMask, i);
                }

                sum += NumberOfSetBits(num);
            }

            return sum;
        }

        private static long NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56;
        }
    }
}

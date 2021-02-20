using Chaos.NaCl;
using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Identity;

namespace O10.Node.Core.Tests
{
    public static class PacketsBuilder
    {
        public static RegistryRegisterBlock GetTransactionRegisterBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, LedgerType referencedLedgerType, 
            ushort referencedBlockType, byte[] referencedBlockHash, byte[] referencedTarget, byte[] privateKey)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);
            RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
            {
                SyncHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash??new byte[Globals.POW_HASH_SIZE],
                Height = blockHeight,
                ReferencedLedgerType = referencedLedgerType,
                ReferencedBlockType = referencedBlockType,
                ReferencedBodyHash = referencedBlockHash,
                ReferencedTarget = referencedTarget,
                Source = new Key32(publicKey)
            };

            return transactionRegisterBlock;
        }

        public static RegistryShortBlock GetTransactionsShortBlock(ulong syncBlockHeight, uint nonce, byte[] powHash, ulong blockHeight, byte round, IEnumerable<RegistryRegisterBlock> transactionRegisterBlocks, byte[] privateKey)
        {
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);

            WitnessStateKey[] transactionHeaders = new WitnessStateKey[transactionRegisterBlocks.Count()];

            ushort order = 0;
            foreach (var item in transactionRegisterBlocks)
            {
                transactionHeaders[order++] = new WitnessStateKey { PublicKey = item.Source, Height = item.Height };
            }

            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash ?? new byte[Globals.POW_HASH_SIZE],
                Height = blockHeight,
                WitnessStateKeys = transactionHeaders,
                Source = new Key32(publicKey)
            };

            return transactionsShortBlock;
        }
    }
}

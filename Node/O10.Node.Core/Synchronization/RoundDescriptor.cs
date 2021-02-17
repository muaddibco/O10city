﻿using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Core.Identity;

namespace O10.Node.Core.Synchronization
{
    public class RoundDescriptor
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public RoundDescriptor(IIdentityKeyProvider identityKeyProvider)
        {
            CandidateBlocks = new Dictionary<IKey, RegistryFullBlock>();
            CandidateVotes = new Dictionary<IKey, int>();
            VotingBlocks = new HashSet<RegistryConfidenceBlock>();
            _identityKeyProvider = identityKeyProvider;
        }
        
        public Dictionary<IKey, RegistryFullBlock> CandidateBlocks { get; }
        public Dictionary<IKey, int> CandidateVotes { get; }
        public HashSet<RegistryConfidenceBlock> VotingBlocks { get; }

        public void AddFullBlock(RegistryFullBlock registryFullBlock)
        {
            IKey key = _identityKeyProvider.GetKey(registryFullBlock.ShortBlockHash);
            if (!CandidateBlocks.ContainsKey(key))
            {
                CandidateBlocks.Add(key, registryFullBlock);
                CandidateVotes.Add(key, 0);
            }
        }

        public void AddVotingBlock(RegistryConfidenceBlock registryConfidenceBlock)
        {
            VotingBlocks.Add(registryConfidenceBlock);
        }

        public void Reset()
        {
            CandidateBlocks.Clear();
            CandidateVotes.Clear();
            VotingBlocks.Clear();
        }
    }
}

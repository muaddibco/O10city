﻿using System;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
	public class IssueBlindedAssetParser : TransactionalBlockParserBase
	{
		public IssueBlindedAssetParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
		{
		}

		public override ushort BlockType => PacketTypes.Transaction_IssueBlindedAsset;

		protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
		{
			IssueBlindedAsset block = null;

			if (version == 1)
			{
				int readBytes = 0;

                byte[] groupId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
				readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

				byte[] keyImage = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
				readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

				byte[] ringSignatureC = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
				readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

				byte[] ringSignatureR = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
				readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

				block = new IssueBlindedAsset
				{
                    GroupId = groupId,
					AssetCommitment = assetCommitment,
					KeyImage = keyImage,
					UniquencessProof = new RingSignature
					{
						C = ringSignatureC,
						R = ringSignatureR
					}
				};

				transactionalBlockBase = block;
				return spanBody.Slice(readBytes);
			}

			throw new BlockVersionNotSupportedException(version, BlockType);
		}
	}
}

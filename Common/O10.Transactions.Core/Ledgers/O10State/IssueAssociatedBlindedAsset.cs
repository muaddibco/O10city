﻿using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    /// <summary>
    /// Associated binded asset is asset that represent ZK proof for some attribute and it is not issued independently but only if base attribute already issued.
    /// For instance base attribute is ID card number and associated attribute is First Name of a user
    /// In order to request assiciated attribute commitment user send a requesting transaction with following details:
    ///   1. Commitment and Surjection Proof to base attribute
    ///   2. Commitment that will be stored into AssociatedEncryptedCommitment - that is commitment to the same base attribute commitment
    /// </summary>
    public class IssueAssociatedBlindedAsset : TransactionalPacketBase
	{
		public override ushort Version => 1;

		public override ushort PacketType => PacketTypes.Transaction_IssueAssociatedBlindedAsset;

        public byte[] AssetCommitment { get; set; }

		/// <summary>
		/// Contains Commitment produced from another original commitment: C` = C + r`*G
		/// EcdhTuple contains additional blinding factor r` and original commitment C
		/// </summary>
		public byte[] RootAssetCommitment { get; set; }
	}
}
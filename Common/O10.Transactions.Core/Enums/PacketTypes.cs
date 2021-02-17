namespace O10.Transactions.Core.Enums
{
    public static class PacketTypes
    {
		/// <summary>
		/// This flag means that transaction is intended for transferring from State based account to UTXO one
		/// </summary>
		public const ushort TransitionalFlag = 0xF000;

        public const ushort Unknown = 0;
        public const ushort Registry_Register = 1;
        public const ushort Registry_FullBlock = 2;
        public const ushort Registry_ShortBlock = 3;
        public const ushort Registry_ConfidenceBlock = 4;
        public const ushort Registry_ConfirmationBlock = 5;
        public const ushort Registry_RegisterStealth = 6;
		public const ushort Registry_RegisterTransfer = 7;
        public const ushort Registry_RegisterEx = 8;

        public const ushort Transaction_UniversalTransport = 1;
        public const ushort Transaction_IssueAssociatedAsset = 6;
        public const ushort Transaction_IssueAsset = 7;
		public const ushort Transaction_TransferAsset = 8;
		public const ushort Transaction_transferAssetToStealth = 8 + TransitionalFlag;
        public const ushort Transaction_BlindAsset = 9;
        public const ushort Transaction_RetransferAssetToStealth = 10 + TransitionalFlag;
        public const ushort Transaction_IssueBlindedAsset = 11;
        public const ushort Transaction_IssueAssociatedBlindedAsset = 12;
        public const ushort Transaction_EmployeeRecord = 13;
        public const ushort Transaction_DocumentRecord = 14;
        public const ushort Transaction_DocumentSignRecord = 15;
        public const ushort Transaction_CancelEmployeeRecord = 16;

        public const ushort Stealth_IdentityProofs = 1;
        public const ushort Stealth_RevokeIdentity = 2;
        public const ushort Stealth_OnboardingRequest = 6;
        public const ushort Stealth_AuthenticationRequest = 7;
        public const ushort Stealth_EmployeeReqistrationRequest = 8;
        public const ushort Stealth_DocumentSignRequest = 9;
        public const ushort Stealth_GroupsRelationsProofs = 10;
        public const ushort Stealth_UniversalTransport = 11;
        public const ushort Stealth_TransitionCompromisedProofs = ushort.MaxValue;

		public const ushort Consensus_GenericConsensus = ushort.MaxValue;

        public const ushort Synchronization_RegistryCombinationBlock = 5;
        public const ushort Synchronization_ConfirmedBlock = ushort.MaxValue;

        public const ushort Storage_TransactionFull = 1;
    }
}
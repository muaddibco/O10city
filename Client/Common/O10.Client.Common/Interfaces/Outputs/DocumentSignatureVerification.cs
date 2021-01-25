namespace O10.Client.Common.Interfaces.Outputs
{
	public class DocumentSignatureVerification
	{
        public bool IsNotCompromised { get; set; }
        public bool SignatureTransactionFound { get; set; }

		public bool DocumentRecordTransactionFound { get; set; }

		public bool DocumentHashMatch { get; set; }

		public bool EligibilityCorrect { get; set; }

		public bool SignerSignatureMatch { get; set; }

		public bool AllowedGroupRelation { get; set; }
		public bool AllowedGroupMatching { get; set; }
	}
}

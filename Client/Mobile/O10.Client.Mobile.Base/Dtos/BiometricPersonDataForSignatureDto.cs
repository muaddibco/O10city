namespace O10.Client.Mobile.Base.Dtos
{
    public class BiometricPersonDataForSignatureDto
    {
        public string ImageSource { get; set; }

        public string ImageTarget { get; set; }

        public string SourceImageCommitment { get; set; }

        public string SourceImageProofCommitment { get; set; }

        public string SourceImageProofSignatureE { get; set; }

        public string SourceImageProofSignatureS { get; set; }

        public string AuxMessage { get; set; }
    }
}

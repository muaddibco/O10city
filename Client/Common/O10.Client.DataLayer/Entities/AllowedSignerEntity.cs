namespace O10.Client.DataLayer.Entities
{
    public class AllowedSignerEntity
    {
        public byte[] GroupCommitment { get; set; }
        public byte[] BlindingFactor { get; set; }
        public string GroupIssuer { get; set; }
        public string GroupName { get; set; }
    }
}

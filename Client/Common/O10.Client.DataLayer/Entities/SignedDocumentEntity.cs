using System.Collections.Generic;

namespace O10.Client.DataLayer.Entities
{
    public class SignedDocumentEntity
    {
        public long DocumentId { get; set; }
        public string DocumentName { get; set; }
        public ulong LastChangeRecordHeight { get; set; }
        public string Hash { get; set; }

        public List<AllowedSignerEntity> AllowedSigners { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("AssociatedAttributeBackups")]
    public class AssociatedAttributeBackup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AssociatedAttributeBackupId { get; set; }

        public string RootIssuer { get; set; }
        public string RootAssetId { get; set; }
        public string AssociatedIssuer { get; set; }
        public string SchemeName { get; set; }
        public string Content { get; set; }
    }
}

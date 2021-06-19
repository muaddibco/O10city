using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
    [Table("RelationRecords")]
    public class RelationRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RelationRecordId { get; set; }

        public Account Account { get; set; }

        public string Description { get; set; }

        [Required]
        public string RootAttributeValue { get; set; }

        public RegistrationCommitment RegistrationCommitment { get; set; }

        public RelationGroup RelationGroup { get; set; }
    }
}

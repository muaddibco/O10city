using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("AccountKeyValues")]
    public class AccountKeyValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountKeyValueId { get; set; }

        public long AccountId { get; set; }

        [MaxLength(255)]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}

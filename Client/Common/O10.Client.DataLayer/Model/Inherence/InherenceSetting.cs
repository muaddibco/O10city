using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Inherence
{
    [Table("InherenceSettings")]
    public class InherenceSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long InherenceSettingId { get; set; }

        [Required]
        public string Name { get; set; }

        public long AccountId { get; set; }
    }
}

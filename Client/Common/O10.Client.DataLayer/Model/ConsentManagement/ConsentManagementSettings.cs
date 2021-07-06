using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.ConsentManagement
{
    [Table("ConsentManagementSettings")]
    public class ConsentManagementSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ConsentManagementSettingsId { get; set; }

        public long AccountId { get; set; }
    }
}

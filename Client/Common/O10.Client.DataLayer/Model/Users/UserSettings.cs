using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("UserSettings")]
    public class UserSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserSettingsId { get; set; }

        public Account? Account { get; set; }

        public bool IsAutoTheftProtection { get; set; }
    }
}

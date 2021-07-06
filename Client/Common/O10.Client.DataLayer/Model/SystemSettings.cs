using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("SystemSettings")]
    public class SystemSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SystemSettingsId { get; set; }

        public byte[] InitializationVector { get; set; }

        public byte[] BiometricSecretKey { get; set; }
    }
}

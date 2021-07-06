using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("AutoLogins")]
    public class AutoLogin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AutoLoginId { get; set; }

        public byte[] SecretKey { get; set; }

        public Account Account { get; set; }
    }
}

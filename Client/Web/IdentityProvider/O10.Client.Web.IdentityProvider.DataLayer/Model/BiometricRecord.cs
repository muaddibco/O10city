using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.IdentityProvider.DataLayer.Model.Enums;

namespace O10.IdentityProvider.DataLayer.Model
{
    [Table("biometric_record")]
    public class BiometricRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BiometricRecordId { get; set; }

        public long UserRecordId { get; set; }

        public BiometricRecordType RecordType { get; set; }

        public byte[] Content { get; set; }
    }
}

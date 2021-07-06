using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("BiometricRecords")]
    public class BiometricRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BiometricRecordId { get; set; }

        public string UserData { get; set; }

        public Guid PersonGuid { get; set; }
    }
}

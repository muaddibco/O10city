using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("sp_employee_groups")]
    public class SpEmployeeGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SpEmployeeGroupId { get; set; }

        public Account Account { get; set; }

        public string GroupName { get; set; }
    }
}

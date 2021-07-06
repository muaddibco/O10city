using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Demo
{
    [Table("WorkSpaces")]
    public class WorkSpace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long WorkSpaceId { get; set; }

        public string Name { get; set; }

        public bool CanManageUsers { get; set; }
        public bool CanManageIPs { get; set; }
        public bool CanManageSPs { get; set; }
    }
}

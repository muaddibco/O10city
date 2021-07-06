using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("IdentitiesSchemes")]
    public class IdentitiesScheme
    {
        public long IdentitiesSchemeId { get; set; }

        [Required]
        public string AttributeName { get; set; }

        [Required]
        public string AttributeSchemeName { get; set; }

        public string? Alias { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public bool CanBeRoot { get; set; }

        public string Issuer { get; set; }
    }
}

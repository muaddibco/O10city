﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("saml_service_providers")]
    public class SamlServiceProvider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SamlServiceProviderId { get; set; }

        public string EntityId { get; set; }

        public string SingleLogoutUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace O10.IdentityProvider.DataLayer.Model
{
	[Table("blinding_factor_records")]
	public class BlindingFactorsRecord
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long BlindingFactorsRecordId { get; set; }

		public string IssuanceBlindingFactor { get; set; }
		public string BiometricBlindingFactor { get; set; }
	}
}

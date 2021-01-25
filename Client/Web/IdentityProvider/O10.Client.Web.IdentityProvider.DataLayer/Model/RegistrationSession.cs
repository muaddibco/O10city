using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace O10.IdentityProvider.DataLayer.Model
{
	[Table("registration_sessions")]
	public class RegistrationSession
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long RegistrationSessionId { get; set; }

		public UserRecord UserRecord { get; set; }

		public string SessionKey { get; set; }

		public string SessionCommitment { get; set; }

		public DateTime CreationTime { get; set; }
	}
}

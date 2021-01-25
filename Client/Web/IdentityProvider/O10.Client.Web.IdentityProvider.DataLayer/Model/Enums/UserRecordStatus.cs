using System;
using System.Collections.Generic;
using System.Text;

namespace O10.IdentityProvider.DataLayer.Model.Enums
{
	public enum UserRecordStatus : byte
	{
		New,
		ConfirmationPending,
		Confirmed
	}
}

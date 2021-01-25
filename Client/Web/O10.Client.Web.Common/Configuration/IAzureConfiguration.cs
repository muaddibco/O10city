using System;
using System.Collections.Generic;
using System.Text;
using O10.Core.Configuration;

namespace O10.Client.Web.Common.Configuration
{
	public interface IAzureConfiguration : IConfigurationSection
	{
		string AzureADCertThumbprint { get; set; }
		string KeyVaultName { get; set; }
		string AzureADApplicationId { get; set; }
	}
}

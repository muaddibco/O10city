using System.ComponentModel;

namespace O10.Client.DataLayer.Enums
{
	public enum ValidationType : ushort
	{
        [Description("Match Value")]
		MatchValue = 1,

        [Description("Age (years)")]
		AgeInYears = 2,

        [Description("Included in group")]
        InclusionIntoGroup = 3
    }
}
using O10.Client.DataLayer.Enums;

namespace O10.Client.Common.Entities
{
    public class ValidationCriteria
    {
        public string SchemeName { get; set; }

        public ValidationType ValidationType { get; set; }

        public ushort? NumericCriterion { get; set; }

        public byte[] GroupIdCriterion { get; set; }
    }
}

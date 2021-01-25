using System;

namespace O10.Client.Common.ExternalIdps.BlinkId
{
    public class BlinkIdIdentityRequest : ExternalIdpRequestBase
    {
        public string DocumentNationality { get; set; }
        public string DocumentType { get; set; }

        public string DocumentNumber { get; set; }
        public string LocalIdNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? IssuanceDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IssuerState { get; set; }
        public string Nationality { get; set; }
        public string VehicleType { get; set; }
    }
}

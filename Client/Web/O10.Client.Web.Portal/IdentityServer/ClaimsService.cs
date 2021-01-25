using IdentityModel;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;

namespace O10.Client.Web.Portal.IdentityServer
{
    public class ClaimsService : DefaultClaimsService
    {
        public ClaimsService(IProfileService profile, ILogger<DefaultClaimsService> logger) : base(profile, logger)
        {
        }

        protected override IEnumerable<Claim> GetStandardSubjectClaims(ClaimsPrincipal subject)
        {
            var claims = base.GetStandardSubjectClaims(subject);
            var newClaims = new List<Claim>(claims)
            {
                new Claim(JwtClaimTypes.Name, subject.Identity.Name)
            };
            return newClaims;
        }
    }
}

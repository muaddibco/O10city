using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using O10.Client.Web.Portal.IdentityServer.Data.Models;

namespace O10.Client.Web.Portal.IdentityServer
{
    public class ProfileService : IProfileService
    {
        protected UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject).ConfigureAwait(false);
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Name, user.Email),
                new Claim(JwtClaimTypes.Role, "muaddibco@gmail.com".Equals(user.Email, StringComparison.InvariantCultureIgnoreCase) ? "Admin" : "Standard")
            };

            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var user = await _userManager.GetUserAsync(context.Subject).ConfigureAwait(false);
            context.IsActive = (user != null) && user.LockoutEnabled;
        }
    }
}

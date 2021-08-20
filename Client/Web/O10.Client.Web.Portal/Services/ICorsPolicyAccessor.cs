using Microsoft.AspNetCore.Cors.Infrastructure;

namespace O10.Client.Web.Portal.Services
{
    public interface ICorsPolicyAccessor
    {
        CorsPolicy GetPolicy();

        CorsPolicy GetPolicy(string name);
    }
}

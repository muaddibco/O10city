using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using System;

namespace O10.Client.Web.Portal.Services
{
    public class CorsPolicyAccessor : ICorsPolicyAccessor
    {
        private readonly CorsOptions _options;

        public CorsPolicyAccessor(IOptions<CorsOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
        }

        public CorsPolicy GetPolicy()
        {
            return _options.GetPolicy(_options.DefaultPolicyName);
        }

        public CorsPolicy GetPolicy(string name)
        {
            return _options.GetPolicy(name);
        }
    }
}

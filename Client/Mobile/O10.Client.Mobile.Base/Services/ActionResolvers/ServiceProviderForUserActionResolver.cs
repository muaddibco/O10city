using System;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using System.Web;
using System.Collections.Specialized;
using O10.Core.ExtensionMethods;
using O10.Client.DataLayer.Services;

namespace O10.Client.Mobile.Base.Services.ActionResolvers
{
    [RegisterExtension(typeof(IActionResolver), Lifetime = LifetimeManagement.Singleton)]
    public class ServiceProviderForUserActionResolver : IActionResolver
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IExecutionContext _executionContext;

        public ServiceProviderForUserActionResolver(IDataAccessService dataAccessService, IExecutionContext executionContext)
        {
            _dataAccessService = dataAccessService;
            _executionContext = executionContext;
        }

        public string ResolveAction(string action)
        {
            if (action.StartsWith("spp://"))
            {
                string actionInfo = action.Replace("spp://", "");
                if (Uri.IsWellFormedUriString(actionInfo, UriKind.RelativeOrAbsolute))
                {
                    UriBuilder uriBuilder = new UriBuilder(actionInfo);
                    NameValueCollection query = HttpUtility.ParseQueryString(uriBuilder.Query);
                    string actionType = query["t"];
                    if ("0".Equals(actionType))
                    {
                        return $"ServiceProviderForUser?actionInfo={actionInfo.EncodeToEscapedString64()}";
                    }
                }
            }


            return null;
        }


    }
}

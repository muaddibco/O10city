using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Client.Common.Logging
{
    [RegisterExtension(typeof(ILogger), Lifetime = LifetimeManagement.Transient)]
    public class Log4NetPerAccountLogger : Log4NetLogger
    {
        private readonly IClientContext _clientContext;

        public Log4NetPerAccountLogger(IConfigurationService configurationService, IClientContext clientContext) : base(configurationService)
        {
            _clientContext = clientContext;
        }

        public override void Debug(string msg, params object[] messageArgs)
        {
            base.Debug($"[{_clientContext.AccountId}]: ${msg}", messageArgs);
        }
    }
}

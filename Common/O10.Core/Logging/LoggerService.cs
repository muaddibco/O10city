using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Core.Logging
{
    [RegisterDefaultImplementation(typeof(ILoggerService), Lifetime = LifetimeManagement.Singleton)]
    public class LoggerService : ILoggerService
    {
        private readonly Dictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();
        private readonly ILogConfiguration _logConfiguration;
        private readonly object _sync = new object();
		private readonly IServiceProvider _serviceProvider;

		public LoggerService(IConfigurationService configurationService, IServiceProvider serviceProvider)
        {
            _logConfiguration = configurationService.Get<ILogConfiguration>();
			_serviceProvider = serviceProvider;
		}

        public ILogger GetLogger(string scopeName)
        {
            if(!_loggers.ContainsKey(scopeName))
            {
                lock(_sync)
                {
                    if (!_loggers.ContainsKey(scopeName))
                    {
                        string loggerName = string.IsNullOrEmpty(_logConfiguration?.LoggerName) ? typeof(Log4NetLogger).FullName : _logConfiguration?.LoggerName;
						Type loggerType = Type.GetType(loggerName);
                        
						ILogger logger =  (ILogger)ActivatorUtilities.CreateInstance(_serviceProvider, loggerType);
                        logger.Initialize(scopeName);
                        _loggers.Add(scopeName, logger);
                    }
                }
            }

            return _loggers[scopeName];
        }
    }
}

using log4net;
using log4net.Config;
using log4net.Repository;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.Serialization;

namespace O10.Core.Logging
{
    [RegisterExtension(typeof(ILogger), Lifetime = LifetimeManagement.Transient)]
    public class Log4NetLogger : ILogger
    {
        private readonly IConfigurationService _configurationService;

        private bool _isInitialized;
        private string _logRepositoryName;
		private ILog _log;
        private string? _context;

		private static readonly object _sync = new object();

		public Log4NetLogger(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public bool IsWarnEnabled { get => _log.IsWarnEnabled; }
        public bool IsInfoEnabled { get => _log.IsInfoEnabled; }
        public bool IsDebugEnabled { get => _log.IsDebugEnabled; }
        public bool IsErrorEnabled { get => _log.IsErrorEnabled; }

        public void Initialize(string scopeName, string logConfigurationFile = null, LogLevel logLevel = LogLevel.Info)
        {
            if(!_isInitialized)
            {
                lock(_sync)
                {
                    if(!_isInitialized)
                    {
                        ConfigureLog4Net(logConfigurationFile ?? _configurationService?.Get<ILogConfiguration>()?.LogConfigurationFile);

                        _isInitialized = true;
                    }
                }
            }

            _log = LogManager.GetLogger(_logRepositoryName, scopeName);
        }

        public virtual void Debug(string msg, params object[] messageArgs)
        {
			if(_log.IsDebugEnabled)
            {
                string formattedMessage = FormatMessage(msg, messageArgs);

                _log.Debug(GetContextedMessage(formattedMessage));
            }
        }

        public virtual void Debug(Func<string> getMsg)
		{
			if (_log.IsDebugEnabled && getMsg != null)
			{
				_log.Debug(GetContextedMessage(getMsg()));
			}
		}

		public virtual void Info(string msg, params object[] messageArgs)
        {
			if(_log.IsInfoEnabled)
			{
				string formattedMessage = FormatMessage(msg, messageArgs);
				_log.Info(GetContextedMessage(formattedMessage));
			}
		}

        public virtual void Warning(string msg, params object[] messageArgs)
        {
			if(_log.IsWarnEnabled)
			{
				string formattedMessage = FormatMessage(msg, messageArgs);
				_log.Warn(GetContextedMessage(formattedMessage));
			}
		}

        public virtual void Error(string msg, params object[] messageArgs)
        {
			if(_log.IsErrorEnabled)
			{
				string formattedMessage = FormatMessage(msg, messageArgs);

				_log.Error(GetContextedMessage(formattedMessage));
			}
		}

        public virtual void Error(string msg, Exception ex, params object[] messageArgs)
        {
            if (ex == null)
            {
                Error(msg, messageArgs);
                return;
            }

			if(_log.IsErrorEnabled)
			{
				string formattedMessage = FormatMessage(msg, messageArgs);

				_log.Error(GetContextedMessage(formattedMessage), ex);
			}
		}

        public virtual void LogIfDebug(Func<string> messageFactory, params object[] argsToJson)
        {
            if (_log.IsDebugEnabled)
            {
                string message = messageFactory?.Invoke();
                string formattedMessage = message;
                if(argsToJson != null && argsToJson.Length > 0)
                {
                    string[] jsons = new string[argsToJson.Length];
                    for (int i = 0; i < argsToJson.Length; i++)
                    {
                        jsons[i] = JsonConvert.SerializeObject(argsToJson[i], new ByteArrayJsonConverter());
                    }

                    formattedMessage = string.Format(message, jsons);
                }

                if(!string.IsNullOrEmpty(formattedMessage))
                {
                    _log.Debug(GetContextedMessage(formattedMessage));
                }
            }
        }

        public virtual void LogIfInfo(Func<string> messageFactory)
        {
            if (_log.IsInfoEnabled)
            {
                string message = messageFactory?.Invoke();
                if (!string.IsNullOrEmpty(message))
                {
                    _log.Debug(GetContextedMessage(message));
                }
            }
        }

        private void ConfigureLog4Net(string logConfigFilePath)
        {
            ILoggerRepository loggerRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()??Assembly.GetCallingAssembly());
            _logRepositoryName = loggerRepository.Name;

            if (!string.IsNullOrEmpty(logConfigFilePath))
            {
                FileInfo executinAssemblyInfo = new FileInfo((Assembly.GetEntryAssembly()?? Assembly.GetCallingAssembly()).Location);
                if (executinAssemblyInfo.DirectoryName != null)
                {
                    string path = Path.Combine(executinAssemblyInfo.DirectoryName, logConfigFilePath);
                    if (File.Exists(path))
                    {
                        XmlConfigurator.Configure(loggerRepository, new FileInfo(path));
                    }

                    //throw new FailedToFindLogConfigFileException("log4net", path);

                    //TODO: ascertain XmlConfigurator works as expected
                }
            }
            else
            {
                if(!loggerRepository.Configured)
                {
                    XmlConfigurator.Configure(loggerRepository);
                }
            }
        }

        private static string FormatMessage(string msg, object[] messageArgs)
        {
            if (messageArgs == null || messageArgs.Length == 0)
            {
                return msg ?? string.Empty;
            }

            return string.Format(msg ?? string.Empty, messageArgs);
        }

        public void SetContext(string context)
        {
            _context = context;
        }

        private string GetContextedMessage(string msg)
        {
            return (!string.IsNullOrEmpty(_context) ? $"[{_context}]: " : string.Empty) + msg;
        }
    }
}

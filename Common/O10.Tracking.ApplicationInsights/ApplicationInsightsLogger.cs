using Newtonsoft.Json;
using System;
using O10.Core.Architecture;

using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Core.Serialization;

namespace O10.Tracking.ApplicationInsights
{
    [RegisterExtension(typeof(ILogger), Lifetime = LifetimeManagement.Transient)]
    public class ApplicationInsightsLogger : ILogger
    {
        private readonly ITrackingService _trackingService;
		private LogLevel _logLevel = LogLevel.Info;
		public bool IsWarnEnabled { get => _logLevel <= LogLevel.Warning; }
		public bool IsInfoEnabled { get => _logLevel <= LogLevel.Info; }
		public bool IsDebugEnabled { get => _logLevel == LogLevel.Debug ; }
		public bool IsErrorEnabled { get => true; }

		public ApplicationInsightsLogger(ITrackingService trackingService)
        {
            _trackingService = trackingService;
        }

		public void Debug(string msg, params object[] messageArgs)
		{
			if (_logLevel <= LogLevel.Debug)
			{
				_trackingService.TrackTrace(string.Format(msg, messageArgs));
			}
		}

		public void Debug(Func<string> getMsg)
		{
			if (_logLevel <= LogLevel.Debug && getMsg != null)
			{
				_trackingService.TrackTrace(getMsg());
			}
		}

		public void Error(string msg, params object[] messageArgs)
		{
			if (_logLevel <= LogLevel.Error)
			{
				_trackingService.TrackTrace(string.Format(msg, messageArgs));
			}
		}

		public void Error(string msg, Exception ex, params object[] messageArgs)
        {
			if (_logLevel <= LogLevel.Error)
			{
				_trackingService.TrackTrace(string.Format(msg, messageArgs));
				_trackingService.TrackException(ex);
			}
        }

		public void Info(string msg, params object[] messageArgs)
		{
			if (_logLevel <= LogLevel.Info)
			{
				_trackingService.TrackTrace(string.Format(msg, messageArgs));
			}
		}

		public void Initialize(string scopeName, string logConfigurationFile = null, LogLevel logLevel = LogLevel.Info)
        {
			_logLevel = logLevel;
		}

		public void Warning(string msg, params object[] messageArgs)
		{
			if (_logLevel <= LogLevel.Warning)
			{
				_trackingService.TrackTrace(string.Format(msg, messageArgs));
			}
		}

		public void LogIfDebug(Func<string> messageFactory, params object[] argsToJson)
		{
			if(_logLevel == LogLevel.Debug)
			{
				string message = messageFactory?.Invoke();
				string formattedMessage = message;
				if (argsToJson != null && argsToJson.Length > 0)
				{
					string[] jsons = new string[argsToJson.Length];
					for (int i = 0; i < argsToJson.Length; i++)
					{
						jsons[i] = JsonConvert.SerializeObject(argsToJson[i], new ByteArrayJsonConverter());
					}

					formattedMessage = string.Format(message, jsons);
				}

				if (!string.IsNullOrEmpty(formattedMessage))
				{
					_trackingService.TrackTrace(formattedMessage);
				}
			}
		}

		public void LogIfInfo(Func<string> messageFactory)
		{
			if (_logLevel <= LogLevel.Info)
			{
				string message = messageFactory?.Invoke();
				if (!string.IsNullOrEmpty(message))
				{
					_trackingService.TrackTrace(message);
				}
			}
		}
	}
}

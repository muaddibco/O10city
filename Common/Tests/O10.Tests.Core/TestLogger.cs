using Newtonsoft.Json;
using System;
using O10.Core.Logging;
using Xunit.Abstractions;
using O10.Core.Serialization;

namespace O10.Tests.Core
{
    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public bool IsWarnEnabled => true;

        public bool IsInfoEnabled => true;

        public bool IsDebugEnabled => true;

        public bool IsErrorEnabled => true;

        public void Debug(string msg, params object[] messageArgs)
        {
            _testOutputHelper.WriteLine("[DEBUG] " + msg, messageArgs);
        }

        public void Debug(Func<string> getMsg)
        {
            string msg = getMsg?.Invoke();

            _testOutputHelper.WriteLine("[DEBUG] " + msg);
        }

        public void Error(string msg, params object[] messageArgs)
        {
            _testOutputHelper.WriteLine("[ERROR] " + msg, messageArgs);
        }

        public void Error(string msg, Exception ex, params object[] messageArgs)
        {
            _testOutputHelper.WriteLine("[ERROR] " + msg + "\r\n" + ex?.ToString(), messageArgs);
        }

        public void Info(string msg, params object[] messageArgs)
        {
            _testOutputHelper.WriteLine("[INFO] " + msg, messageArgs);
        }

        public void Initialize(string scopeName, string logConfigurationFile = null, LogLevel logLevel = LogLevel.Info)
        {
        }

        public void LogIfDebug(Func<string> messageFactory, params object[] argsToJson)
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

            _testOutputHelper.WriteLine("[DEBUG] " + formattedMessage);
        }

        public void LogIfInfo(Func<string> messageFactory)
        {
            string msg = messageFactory?.Invoke();
            _testOutputHelper.WriteLine("[INFO] " + msg);
        }

        public void SetContext(string context)
        {
        }

        public void Warning(string msg, params object[] messageArgs)
        {
            _testOutputHelper.WriteLine("[WARN] " + msg, messageArgs);
        }
    }
}

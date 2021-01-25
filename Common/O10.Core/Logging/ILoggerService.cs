using O10.Core.Architecture;

namespace O10.Core.Logging
{
    [ServiceContract]
    public interface ILoggerService
    {
        ILogger GetLogger(string scopeName);
    }
}

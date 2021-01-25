using O10.Core.Architecture;

namespace O10.Core.Configuration
{
    [ServiceContract]
    public interface IAppConfig
    {
        string GetString(string key, bool required = true);

        long GetLong(string key, bool required = true);

        bool GetBool(string key, bool required = true);

        string ReplaceToken(string src);
    }
}

using System.Numerics;
using System.Threading.Tasks;

namespace O10.Client.Common.Integration
{
    public interface IIntegration
    {
        string Key { get; }

        void Initialize();

        string GetAddress(long accountId);
        Task<BigInteger> GetBalance(long accountId);
    }
}

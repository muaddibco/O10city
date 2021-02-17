using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Models;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IBiometricService
    {
        Task<BiometricProof> CheckBiometrics(string imageContent, RootAttributeModel rootAttribute, byte[] bindingKey);
    }
}

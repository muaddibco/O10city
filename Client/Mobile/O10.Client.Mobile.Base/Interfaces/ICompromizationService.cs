using System.Threading.Tasks;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface ICompromizationService
    {
        Task ProcessCompromization(byte[] keyImage, byte[] transactionKey, byte[] destinationKey, byte[] target);

        bool IsProtectionEnabled { get; set; }
    }
}

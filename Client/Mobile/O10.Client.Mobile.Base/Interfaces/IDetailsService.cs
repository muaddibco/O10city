using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Models;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IDetailsService
    {
        Task<IssuerActionDetails> GetActionDetails(string uri);

        Task<ServiceProviderActionAndValidations> GetServiceProviderActionAndValidations(string uri);
    }
}

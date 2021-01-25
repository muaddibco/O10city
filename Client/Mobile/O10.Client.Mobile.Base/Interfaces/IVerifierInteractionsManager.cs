using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IVerifierInteractionsManager : IRepository<IVerifierInteractionService, string>
    {
        Task Initialize();

        IEnumerable<InherenceServiceInfo> GetInherenceServices();
    }
}

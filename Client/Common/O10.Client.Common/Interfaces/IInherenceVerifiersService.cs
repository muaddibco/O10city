using System.Collections.Generic;
using System.Threading.Tasks;
using O10.Client.Common.Dtos;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IInherenceVerifiersService
    {
        Task<IEnumerable<InherenceServiceInfoDTO>> GetInherenceServices();
    }
}

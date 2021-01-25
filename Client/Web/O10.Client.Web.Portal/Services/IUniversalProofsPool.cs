using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Architecture;
using O10.Core.Identity;

namespace O10.Client.Web.Portal.Services
{
    [ServiceContract]
    public interface IUniversalProofsPool
    {
        public void Store(UniversalProofs universalProofs);

        public TaskCompletionSource<UniversalProofs> Extract(IKey keyImage);
    }
}

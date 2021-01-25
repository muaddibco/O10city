using O10.Core.Architecture;
using O10.Gateway.WebApp.Common.Models;

namespace O10.Gateway.WebApp.Common.Services
{
    [ServiceContract]
    public interface IRelationProofSessionsStorage
    {
        string Push(RelationProofSession relationProofSession);

        RelationProofSession Pop(string sessionKey);
    }
}

using System.Collections.Concurrent;
using O10.Core.ExtensionMethods;
using O10.Core.Architecture;
using O10.Crypto.ConfidentialAssets;
using O10.Gateway.WebApp.Common.Models;

namespace O10.Gateway.WebApp.Common.Services

{
    [RegisterDefaultImplementation(typeof(IRelationProofSessionsStorage), Lifetime = LifetimeManagement.Singleton)]
    public class RelationProofSessionsStorage : IRelationProofSessionsStorage
    {
        private readonly ConcurrentDictionary<string, RelationProofSession> _relationProofSessions = new ConcurrentDictionary<string, RelationProofSession>();

        public RelationProofSession Pop(string sessionKey)
        {
            if (_relationProofSessions.TryRemove(sessionKey, out RelationProofSession relationProofSession))
            {
                return relationProofSession;
            }

            return null;
        }

        public string Push(RelationProofSession relationProofSession)
        {
            string sessionKey = CryptoHelper.GetRandomSeed().ToHexString();
            _relationProofSessions.AddOrUpdate(sessionKey, relationProofSession, (k, v) => v);

            return sessionKey;
        }
    }
}

using O10.Client.DataLayer.ElectionCommittee;
using O10.Client.Web.Portal.ElectionCommittee.Models;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.Translators;
using O10.Core.ExtensionMethods;
using System;
using System.Linq;

namespace O10.Client.Web.Portal.ElectionCommittee.Translators
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class DbToPollTranslator : TranslatorBase<EcPollRecord, Poll>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public DbToPollTranslator(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override Poll Translate(EcPollRecord obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new Poll
            {
                PollId = obj.EcPollRecordId,
                Name = obj.Name,
                State = (PollState)obj.State,
                Candidates = obj.Candidates?.Select(c => new Candidate
                {
                    CandidateId = c.EcCandidateRecordId,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    AssetId = _identityKeyProvider.GetKey(c.AssetId.HexStringToByteArray())
                }).ToList()
            };
        }
    }
}

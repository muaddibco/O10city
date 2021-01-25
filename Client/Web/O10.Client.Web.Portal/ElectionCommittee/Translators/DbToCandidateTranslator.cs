using O10.Client.DataLayer.ElectionCommittee;
using O10.Client.Web.Portal.ElectionCommittee.Models;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using O10.Core.Translators;
using System;
using O10.Core.Architecture;


namespace O10.Client.Web.Portal.ElectionCommittee.Translators
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class DbToCandidateTranslator : TranslatorBase<EcCandidateRecord, Candidate>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public DbToCandidateTranslator(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override Candidate Translate(EcCandidateRecord obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new Candidate
            {
                CandidateId = obj.EcCandidateRecordId,
                Name = obj.Name,
                IsActive = obj.IsActive,
                AssetId = _identityKeyProvider.GetKey(obj.AssetId.HexStringToByteArray())
            };
        }
    }
}

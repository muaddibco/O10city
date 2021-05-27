using System;
using System.Collections.Generic;
using System.Linq;
using O10.Client.DataLayer.Model;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterSimulatorImplementation(typeof(IDataAccessServiceWrapper), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessServiceWrapperSimulator : IDataAccessServiceWrapper
    {
        private readonly Dictionary<long, List<UserRootAttribute>> _rootAttrs = new Dictionary<long, List<UserRootAttribute>>();
        private readonly Dictionary<string, string> _issuers = new Dictionary<string, string>();
        private readonly Dictionary<string, List<UserAssociatedAttribute>> _associatedAttrs = new Dictionary<string, List<UserAssociatedAttribute>>();

        public DataAccessServiceWrapperSimulator()
        {
            _issuers.Add(CryptoHelper.GetRandomSeed().ToHexString(), "Trusted Idenity Provider");
            _issuers.Add(CryptoHelper.GetRandomSeed().ToHexString(), "O10 IdP");

            _associatedAttrs.Add(_issuers.Keys.ElementAt(0),
                new List<UserAssociatedAttribute>
                {
                    new UserAssociatedAttribute
                    {
                        AttributeSchemeName = "FirstName",
                        Content = "Kirill",
                        Source = _issuers.Keys.ElementAt(0)
                    },
                    new UserAssociatedAttribute
                    {
                        AttributeSchemeName = "LastName",
                        Content = "Gandyl",
                        Source = _issuers.Keys.ElementAt(0)
                    }
                });
        }

        public void AddOrUpdateUserIdentityIsser(string key, string issuerAlias, string description)
        {
        }

        public IEnumerable<(string schemeName, string content)> GetUserAssociatedAttributes(long accountId, string issuer)
        {
            if (_associatedAttrs.ContainsKey(issuer))
            {
                return _associatedAttrs[issuer].Select(a => (a.AttributeSchemeName, a.Content));
            }

            return Enumerable.Empty<(string, string)>();
        }

        public IEnumerable<UserRootAttribute> GetUserAttributes(long accountId)
        {
            if (!_rootAttrs.ContainsKey(accountId))
            {
                _rootAttrs.Add(accountId,
                    new List<UserRootAttribute>
                    {
                        new UserRootAttribute
                        {
                            AccountId = accountId,
                            AssetId = CryptoHelper.GetRandomSeed(),
                            Content = "3334567982",
                            ConfirmationTime = DateTime.Now,
                            CreationTime = DateTime.Now,
                            IsOverriden = false,
                            IssuanceCommitment = CryptoHelper.GetRandomSeed(),
                            LastBlindingFactor = CryptoHelper.GetRandomSeed(),
                            LastCommitment = CryptoHelper.GetRandomSeed(),
                            LastDestinationKey = CryptoHelper.GetRandomSeed(),
                            LastTransactionKey = CryptoHelper.GetRandomSeed(),
                            LastUpdateTime = DateTime.Now,
                            NextKeyImage = CryptoHelper.GetRandomSeed().ToHexString(),
                            OriginalBlindingFactor = CryptoHelper.GetRandomSeed(),
                            OriginalCommitment = CryptoHelper.GetRandomSeed(),
                            SchemeName = "ID Card",
                            Source = _issuers.Keys.ElementAt(0),
                            UserAttributeId = 1
                        },
                        new UserRootAttribute
                        {
                            AccountId = accountId,
                            AssetId = CryptoHelper.GetRandomSeed(),
                            Content = "qqq@gmail.com",
                            ConfirmationTime = DateTime.Now,
                            CreationTime = DateTime.Now,
                            IsOverriden = false,
                            IssuanceCommitment = CryptoHelper.GetRandomSeed(),
                            LastBlindingFactor = CryptoHelper.GetRandomSeed(),
                            LastCommitment = CryptoHelper.GetRandomSeed(),
                            LastDestinationKey = CryptoHelper.GetRandomSeed(),
                            LastTransactionKey = CryptoHelper.GetRandomSeed(),
                            LastUpdateTime = DateTime.Now,
                            NextKeyImage = CryptoHelper.GetRandomSeed().ToHexString(),
                            OriginalBlindingFactor = CryptoHelper.GetRandomSeed(),
                            OriginalCommitment = CryptoHelper.GetRandomSeed(),
                            SchemeName = "Email",
                            Source = _issuers.Keys.ElementAt(1),
                            UserAttributeId = 1
                        }
                    });
            }
            return _rootAttrs[accountId];
        }

        public string GetUserIdentityIsserAlias(string key)
        {
            return _issuers[key];
        }
    }
}

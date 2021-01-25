using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms.Internals;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    [RegisterDefaultImplementation(typeof(IVerifierInteractionsManager), Lifetime = LifetimeManagement.Singleton)]
    public class VerifierInteractionsManager : IVerifierInteractionsManager
    {
        private readonly IInherenceVerifiersService _inherenceVerifiersService;
        private readonly IEnumerable<IVerifierInteractionService> _verifierInteractionServices;
        private IEnumerable<InherenceServiceInfo> _inherenceServiceInfos;

        public VerifierInteractionsManager(IInherenceVerifiersService inherenceVerifiersService, IEnumerable<IVerifierInteractionService> verifierInteractionServices)
        {
            _inherenceVerifiersService = inherenceVerifiersService;
            _verifierInteractionServices = verifierInteractionServices;
        }

        public IEnumerable<InherenceServiceInfo> GetInherenceServices() => _inherenceServiceInfos;

        public IVerifierInteractionService GetInstance(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("Name of Verifier Interaction Service cannot be empty", nameof(key));
            }

            if (_inherenceServiceInfos.Any(s => s.Name == key))
            {
                return _verifierInteractionServices?.FirstOrDefault(s => s.Name == key);
            }

            throw new ArgumentOutOfRangeException(nameof(key), $"There is no Inherence Service with key '{key}'");
        }

        public async Task Initialize()
        {
            _inherenceServiceInfos = await _inherenceVerifiersService.GetInherenceServices().ConfigureAwait(false);
            _inherenceServiceInfos.ForEach(s =>
            {
                var interactionService = _verifierInteractionServices.FirstOrDefault(a => a.Name == s.Name);
                if (interactionService != null)
                {
                    interactionService.ServiceInfo = s;
                }
            });
        }
    }
}

using System.Collections.Generic;
using O10.Core.Architecture;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace O10.Network.Handlers
{
    [RegisterDefaultImplementation(typeof(ICoreVerifiersBulkFactory), Lifetime = LifetimeManagement.Singleton)]
    internal class CoreVerifiersBulkFactory : ICoreVerifiersBulkFactory
    {
        private readonly IEnumerable<ICoreVerifier> _coreVerifiers;
        private readonly IServiceProvider _serviceProvider;

        public CoreVerifiersBulkFactory(IEnumerable<ICoreVerifier> coreVerifiers, IServiceProvider serviceProvider)
        {
            _coreVerifiers = coreVerifiers;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<ICoreVerifier> Create()
        {
            List<ICoreVerifier> coreVerifiers = new List<ICoreVerifier>();

            if (_coreVerifiers != null)
            {
                foreach (ICoreVerifier coreVerifier in _coreVerifiers)
                {
                    coreVerifiers.Add((ICoreVerifier)ActivatorUtilities.CreateInstance(_serviceProvider, coreVerifier.GetType()));
                }
            }

            return coreVerifiers;
        }
    }
}

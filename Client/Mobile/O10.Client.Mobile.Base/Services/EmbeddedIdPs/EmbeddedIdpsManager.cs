using System;
using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;

namespace O10.Client.Mobile.Base.Services.EmbeddedIdPs
{
    [RegisterDefaultImplementation(typeof(IEmbeddedIdpsManager), Lifetime = LifetimeManagement.Singleton)]
    public class EmbeddedIdpsManager : IEmbeddedIdpsManager
    {
        private readonly IEnumerable<IEmbeddedIdpService> _embeddedIdpServices;

        public EmbeddedIdpsManager(IEnumerable<IEmbeddedIdpService> embeddedIdpServices)
        {
            _embeddedIdpServices = embeddedIdpServices;
        }

        public IEnumerable<IEmbeddedIdpService> GetAllServices()
        {
            return _embeddedIdpServices;
        }

        public IEmbeddedIdpService GetInstance(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("Name of Embedded IdP Service cannot be empty", nameof(key));
            }

            return _embeddedIdpServices?.FirstOrDefault(s => s.Name == key);
        }
    }
}

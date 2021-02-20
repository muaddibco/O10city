using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O10.Transactions.Core.Accessors
{
    [RegisterDefaultImplementation(typeof(IAccessorProvider), Lifetime = LifetimeManagement.Singleton)]
    public class AccessorProvider : IAccessorProvider
    {
        private readonly IEnumerable<IAccessor> _accessors;

        public AccessorProvider(IEnumerable<IAccessor> accessors)
        {
            _accessors = accessors;
        }

        public IAccessor GetInstance(LedgerType key)
        {
            var accessor = _accessors.FirstOrDefault(a => a.LedgerType == key);

            if(accessor == null)
            {
                throw new ArgumentOutOfRangeException($"No Accessor found for the key {key}");
            }

            return accessor;
        }
    }
}

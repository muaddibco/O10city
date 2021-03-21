using System;
using System.Collections.Generic;
using System.Linq;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.DataLayer.DataServices
{
    public static class ChainDataServiceExtensions
    {
        public static IEnumerable<T> Get<T>(this IChainDataService service, IDataKey key) where T : IPacketBase
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IEnumerable<IPacketBase> packets = service.Get(key);

            if(packets != null)
            {
                return packets.Cast<T>();
            }

            return null;
        }

        public static T Single<T>(this IChainDataService service, IDataKey key) where T : IPacketBase
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IEnumerable<IPacketBase> packets = service.Get(key);

            if (packets != null)
            {
                return packets.Cast<T>().SingleOrDefault();
            }

            return default;
        }
    }
}

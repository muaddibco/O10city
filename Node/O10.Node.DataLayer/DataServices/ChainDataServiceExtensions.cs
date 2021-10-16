using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.DataLayer.DataServices
{
    public static class ChainDataServiceExtensions
    {
        public static async Task<IEnumerable<T>> Get<T>(this IChainDataService service, IDataKey key, CancellationToken cancellationToken) where T : IPacketBase
        {
            if (service is null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IEnumerable<IPacketBase> packets = await service.Get(key, cancellationToken);

            if(packets != null)
            {
                return packets.Cast<T>();
            }

            return null;
        }

        public static async Task<T> Single<T>(this IChainDataService service, IDataKey key, CancellationToken cancellationToken) where T : IPacketBase
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            IEnumerable<IPacketBase> packets = await service.Get(key, cancellationToken);

            if (packets != null)
            {
                return packets.Cast<T>().SingleOrDefault();
            }

            return default;
        }
    }
}

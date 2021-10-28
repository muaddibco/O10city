using System.Collections.Generic;
using System.Threading;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Models;
using System.Threading.Tasks;

namespace O10.Node.DataLayer.DataServices
{
    public interface IDataService<T> where T : class, ISerializableEntity
    {
        Task Initialize(CancellationToken cancellationToken);

        /// <summary>
        /// Initiates the process of saving of the passed entity and returns a wrapper of a 
        /// TaskCompletionSource where the State is a key that stored entity can be uniquely identified with
        /// (e.g. hash of a packet)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<DataResult<T>> Add(T item);

        Task AddDataKey(IDataKey key, IDataKey newKey, CancellationToken cancellationToken);

        Task<IEnumerable<T>> Get(IDataKey key, CancellationToken cancellationToken);
    }
}

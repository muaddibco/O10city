using System.Collections.Generic;
using System.Threading;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Models;

namespace O10.Node.DataLayer.DataServices
{
	public interface IDataService<T> where T : ISerializableEntity<T>
    {
        void Initialize(CancellationToken cancellationToken);

        void Add(T item);

        IEnumerable<T> Get(IDataKey key);
    }
}

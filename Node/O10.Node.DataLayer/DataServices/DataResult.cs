using O10.Core.Models;
using O10.Node.DataLayer.DataServices.Keys;

namespace O10.Node.DataLayer.DataServices
{
    public class DataResult<T> where T: class, ISerializableEntity
    {
        public DataResult(IDataKey key, T packet)
        {
            Key = key;
            Packet = packet;
        }

        public IDataKey Key { get; }

        public T Packet { get; }
    }
}

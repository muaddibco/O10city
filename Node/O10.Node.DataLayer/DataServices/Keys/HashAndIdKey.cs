using O10.Core.Identity;

namespace O10.Node.DataLayer.DataServices.Keys
{
    public class HashAndIdKey : IdKey
    {
        public HashAndIdKey(IKey hashKey, long id)
            : base(id)
        {
            HashKey = hashKey;
        }
        
        public IKey HashKey { get; }
    }
}

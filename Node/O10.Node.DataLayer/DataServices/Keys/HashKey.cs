using O10.Core.Identity;

namespace O10.Node.DataLayer.DataServices.Keys
{
    public class HashKey : IDataKey
    {
        public HashKey(IKey hash)
        {
            Hash = hash;
        }

        public IKey Hash { get; set; }
    }
}

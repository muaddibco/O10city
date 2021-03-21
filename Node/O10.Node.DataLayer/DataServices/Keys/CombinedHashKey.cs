using O10.Core.Identity;

namespace O10.Node.DataLayer.DataServices.Keys
{
    public class CombinedHashKey : IDataKey
    {
        public CombinedHashKey(long combinedBlockHeight, IKey hash)
        {
            CombinedBlockHeight = combinedBlockHeight;
            Hash = hash;
        }

        public long CombinedBlockHeight { get; set; }

        public IKey Hash { get; set; }

    }
}

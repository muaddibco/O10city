using O10.Core.Identity;

namespace O10.Node.DataLayer.DataServices.Keys
{
	public class UniqueKey : IDataKey
    {
        public UniqueKey(IKey key)
        {
            IdentityKey = key;
        }

        public IKey IdentityKey { get; set; }
    }
}

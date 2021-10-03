using O10.Core.Architecture;

namespace O10.Core.Persistency
{
    [ExtensionPoint]
    public interface IDataAccessService
    {
        bool IsInitialized { get; }
        void Initialize();
    }
}

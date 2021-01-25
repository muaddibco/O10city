using O10.Core.Architecture;

namespace O10.Core.DataLayer
{
    [ExtensionPoint]
    public interface IDataAccessService
    {
        bool IsInitialized { get; }
        void Initialize();
    }
}

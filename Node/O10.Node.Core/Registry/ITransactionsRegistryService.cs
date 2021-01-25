using O10.Core.Architecture;

namespace O10.Node.Core.Registry
{
    [ServiceContract]
    public interface ITransactionsRegistryService
    {
        void Start();

        void Stop();

        void Initialize();
    }
}

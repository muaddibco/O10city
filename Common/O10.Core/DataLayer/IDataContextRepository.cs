using O10.Core.Architecture;

namespace O10.Core.DataLayer
{
    [ServiceContract]
    public interface IDataContextRepository
    {
        T1 GetInstance<T1>(string key) where T1 : IDataContext;
    }
}

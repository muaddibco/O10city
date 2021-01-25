using O10.Core.Architecture;

namespace O10.Core.PermanentLayer
{
    [ServiceContract]
    public interface IPermanentLayerServiceFactory
    {
        T GetService<T>();
    }
}

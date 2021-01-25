using O10.Core.Architecture;

namespace O10.Core.States
{
    [ServiceContract]
    public interface IStatesRepository : IRepository<IState, string>
    {
        T GetInstance<T>() where T : class, IState;
    }
}

using O10.Core.Architecture;

namespace O10.Core.Predicates
{
    [ServiceContract]
    public interface IPredicatesRepository : IRepository<IPredicate, string>
    {
    }
}

using O10.Core.Architecture;

namespace O10.Core.Translators
{
    [ServiceContract]
    public interface ITranslatorsRepository : IRepository<ITranslator, string, string>
    {
        ITranslator<TFrom, TTo> GetInstance<TFrom, TTo>();
    }
}

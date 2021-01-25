using System.Collections.Generic;
using O10.Core.Architecture;

using O10.Core.Exceptions;

namespace O10.Core.Translators
{
    [RegisterDefaultImplementation(typeof(ITranslatorsRepository), Lifetime = LifetimeManagement.Singleton)]
    public class TranslatorsRepository : ITranslatorsRepository
    {
        private readonly Dictionary<string, Dictionary<string, ITranslator>> _translatorsPool;

        public TranslatorsRepository(IEnumerable<ITranslator> mappers)
        {
            _translatorsPool = new Dictionary<string, Dictionary<string, ITranslator>>();

            foreach (ITranslator mapper in mappers)
            {
                if(!_translatorsPool.ContainsKey(mapper.Source))
                {
                    _translatorsPool.Add(mapper.Source, new Dictionary<string, ITranslator>());
                }

                if(!_translatorsPool[mapper.Source].ContainsKey(mapper.Target))
                {
                    _translatorsPool[mapper.Source].Add(mapper.Target, mapper);
                }
            }
        }

        public ITranslator<TFrom, TTo> GetInstance<TFrom, TTo>()
        {
            string from = typeof(TFrom).FullName;
            string to = typeof(TTo).FullName;

            ITranslator<TFrom, TTo> translator = GetInstance(from, to) as ITranslator<TFrom, TTo>;

            return translator;
        }

        public ITranslator GetInstance(string from, string to)
        {
            if (!_translatorsPool.ContainsKey(from) || !_translatorsPool[from].ContainsKey(to))
            {
                throw new TranslatorNotFoundException(from, to);
            }

            return _translatorsPool[from][to];
        }
    }
}

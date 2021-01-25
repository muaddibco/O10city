using O10.Core.Architecture;

namespace O10.Core.Translators
{
    public interface ITranslator<TFrom, TTo> : ITranslator
    {
        TTo Translate(TFrom obj);
    }

    [ExtensionPoint]
    public interface ITranslator
    {
        string Source { get; }
        string Target { get; }

        object Translate(object obj);
    }
}

using O10.Core.Architecture;

namespace O10.Core.Predicates
{
    [ExtensionPoint]
    public interface IPredicate
    {
        string Name { get; }

        bool Evaluate(params object[] args);
    }
}

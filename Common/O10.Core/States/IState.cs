using O10.Core.Architecture;

namespace O10.Core.States
{
    [ExtensionPoint]
    public interface IState
    {
        string Name { get; }
    }
}

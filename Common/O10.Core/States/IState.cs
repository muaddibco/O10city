using System;
using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;

namespace O10.Core.States
{
    [ExtensionPoint]
    public interface IState
    {
        string Name { get; }

        IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock);
    }
}

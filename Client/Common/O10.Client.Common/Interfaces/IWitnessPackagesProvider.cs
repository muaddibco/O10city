using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Communication;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IWitnessPackagesProvider
    {
        string Name { get; }

        ISourceBlock<WitnessPackageWrapper> PipeOut { get; }

        bool Initialize(long accountId, CancellationToken cancellationToken);
        Task Start();
    }
}

using O10.Core;
using O10.Core.Architecture;

namespace O10.Client.Common.Interfaces
{
    [ExtensionPoint]
    public interface IPacketsExtractor : IDynamicPipe
    {
        string Name { get; }

        void Initialize(long accountId);
    }
}

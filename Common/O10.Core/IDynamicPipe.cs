using System.Threading.Tasks.Dataflow;

namespace O10.Core
{
    public interface IDynamicPipe
    {
        ISourceBlock<T> GetSourcePipe<T>(string? name = null);
        ITargetBlock<T> GetTargetPipe<T>(string? name = null);
    }
}

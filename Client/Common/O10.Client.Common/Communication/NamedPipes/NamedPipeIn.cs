using System.Threading.Tasks.Dataflow;

namespace O10.Client.Common.Communication.NamedPipes
{
    public class NamedPipeIn<T>
    {
        public string Name { get; set; }
        public ITargetBlock<T> PipeIn { get; }

    }
}

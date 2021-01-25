using O10.Core.Architecture;
using System.Threading;


namespace O10.Core
{
    public abstract class InitializerBase : IInitializer
    {
        private readonly object _sync = new object();

        public bool Initialized { get; private set; }

        public abstract ExtensionOrderPriorities Priority { get; }

        public void Initialize(CancellationToken cancellationToken)
        {
            if(Initialized)
            {
                return;
            }

            lock(_sync)
            {
                if(Initialized)
                {
                    return;
                }

                InitializeInner(cancellationToken);

                Initialized = true;
            }
        }

        protected abstract void InitializeInner(CancellationToken cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.States;

namespace O10.Network.Synchronization
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationContext : ISynchronizationContext
    {
        private readonly ILogger _logger;

        public SynchronizationContext(ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(SynchronizationContext));
        }

        public SynchronizationDescriptor LastBlockDescriptor { get; private set; }

        public SynchronizationDescriptor PrevBlockDescriptor { get; private set; }

        public string Name => nameof(ISynchronizationContext);

        public long LastRegistrationCombinedBlockHeight { get; set; }

        /// <summary>
        /// Utility function that returns median value from provided array
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        public DateTime GetMedianValue(IEnumerable<DateTime> dateTimes)
        {
            IOrderedEnumerable<DateTime> orderedRetransmittedBlocks = dateTimes.OrderBy(v => v);

            int count = orderedRetransmittedBlocks.Count();
            if (count % 2 == 0)
            {
                int indexMidLow = count / 2 - 1;
                int indexMidHigh = count / 2;

                DateTime dtLow = orderedRetransmittedBlocks.ElementAt(indexMidLow);
                DateTime dtHigh = orderedRetransmittedBlocks.ElementAt(indexMidHigh);

                return dtLow.AddSeconds((dtHigh - dtLow).TotalSeconds / 2);
            }
            else
            {
                int index = count / 2;

                return orderedRetransmittedBlocks.ElementAt(index);
            }
        }

        public void UpdateLastSyncBlockDescriptor(SynchronizationDescriptor synchronizationDescriptor)
        {
            if (synchronizationDescriptor == null)
            {
                throw new ArgumentNullException(nameof(synchronizationDescriptor));
            }

            _logger.Info($"UpdateLastSyncBlockDescriptor: {synchronizationDescriptor}");

            lock (this)
            {

                if (LastBlockDescriptor == null || synchronizationDescriptor.BlockHeight > LastBlockDescriptor.BlockHeight)
                {
                    PrevBlockDescriptor = LastBlockDescriptor;
                    LastBlockDescriptor = synchronizationDescriptor;
                }
            }
        }
    }
}

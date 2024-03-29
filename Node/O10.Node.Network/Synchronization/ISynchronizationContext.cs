﻿using System;
using System.Collections.Generic;
using O10.Core.States;

namespace O10.Network.Synchronization
{
    public interface ISynchronizationContext : IState
    {
        SynchronizationDescriptor LastBlockDescriptor { get; }

        SynchronizationDescriptor PrevBlockDescriptor { get; }

        long LastRegistrationCombinedBlockHeight { get; set; }

        /// <summary>
        /// Utility function that returns median value from provided array
        /// </summary>
        /// <param name="dateTimes"></param>
        /// <returns></returns>
        DateTime GetMedianValue(IEnumerable<DateTime> dateTimes);

        void UpdateLastSyncBlockDescriptor(SynchronizationDescriptor synchronizationDescriptor);
    }
}

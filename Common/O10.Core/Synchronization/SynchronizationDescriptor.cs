using System;
using O10.Core.Identity;

namespace O10.Core.Synchronization
{
    public class SynchronizationDescriptor
    {
        public SynchronizationDescriptor(long blockHeight, IKey hash, DateTime medianTime, DateTime updateTime, ushort round)
        {
            BlockHeight = blockHeight;
            Hash = hash;
            MedianTime = medianTime;
            UpdateTime = updateTime;
            Round = round;
        }

        /// <summary>
        /// Last synchronization block obtained from Network
        /// </summary>
        public long BlockHeight { get; private set; }

        public ushort Round { get; set; }

        public IKey Hash { get; private set; }

        public DateTime MedianTime { get; private set; }

        /// <summary>
        /// Local date and time when last synchronization block was obtained
        /// </summary>
        public DateTime UpdateTime { get; private set; }

        public override string ToString()
        {
            return $"[{BlockHeight} @ {UpdateTime}]: {MedianTime}; Hash = {Hash}";
        }
    }
}

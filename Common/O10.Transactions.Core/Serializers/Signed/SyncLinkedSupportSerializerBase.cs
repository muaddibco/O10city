using System;
using System.IO;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Transactions.Core.Ledgers.Synchronization;

namespace O10.Transactions.Core.Serializers.Signed
{
    public abstract class LinkedSerializerBase<T> : SignatureSupportSerializerBase<T> where T : LinkedPacketBase
    {
        public LinkedSerializerBase(IServiceProvider serviceProvider, LedgerType ledgerType, ushort blockType) 
            : base(serviceProvider, packetType, blockType)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            if (_block.HashPrev == null)
            {
                throw new PreviousHashNotProvidedException();
            }

            bw.Write(_block.HashPrev);
        }
    }
}

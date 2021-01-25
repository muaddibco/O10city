using System;
using System.IO;
using O10.Transactions.Core.DataModel;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;

namespace O10.Transactions.Core.Serializers.Signed
{
    public abstract class LinkedSerializerBase<T> : SignatureSupportSerializerBase<T> where T : LinkedPacketBase
    {
        public LinkedSerializerBase(IServiceProvider serviceProvider, PacketType packetType, ushort blockType) 
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

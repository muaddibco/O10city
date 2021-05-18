using O10.Core.Models;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers
{
    /// <summary>
    /// All packetss in all types of ledgers must inherit from this base class
    /// </summary>
    public abstract class PacketBase<TPayload, TTransaction, TSignature> : SerializableEntity<PacketBase<TPayload, TTransaction, TSignature>>, IPacketBase where TPayload : PayloadBase<TTransaction>, new() where TTransaction : TransactionBase where TSignature : SignatureBase
    {
        public PacketBase()
        {
            Payload = new TPayload();
        }

        public TPayload Payload { get; set; }

        public T? With<T>() where T : TTransaction
        {
            return Payload as T;
        }

        public T? AsPacket<T>() where T : class, IPacketBase
        {
            return this as T;
        }

        public TSignature? Signature { get; set; }

        public abstract LedgerType LedgerType { get; }

        PayloadBase<TransactionBase>? IPacketBase.Payload { get => Payload as PayloadBase<TransactionBase>; }

        SignatureBase? IPacketBase.Signature { get => Signature; }
    }

    public interface IPacketBase: ISerializableEntity<IPacketBase>
    {
        LedgerType LedgerType { get; }

        PayloadBase<TransactionBase>? Payload { get; }

        SignatureBase? Signature { get; }

        T? AsPacket<T>() where T: class, IPacketBase;
    }
}

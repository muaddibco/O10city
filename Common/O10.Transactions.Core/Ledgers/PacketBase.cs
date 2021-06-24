using Newtonsoft.Json;
using O10.Core.Models;
using O10.Core.Serialization;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Ledgers
{
    /// <summary>
    /// All packetss in all types of ledgers must inherit from this base class
    /// </summary>
    public abstract class PacketBase<TPayload, TTransaction, TSignature> : SerializableEntity, IPacketBase<TTransaction> where TPayload : PayloadBase<TTransaction>, new() where TTransaction : TransactionBase where TSignature : SignatureBase
    {
        public PacketBase()
        {
            Payload = new TPayload();
        }

        public TPayload Payload { get; set; }

        public T? With<T>() where T : TTransaction
        {
            return Payload.GetTransaction() as T;
        }

        public T? AsPacket<T>() where T : class, IPacketBase
        {
            return this as T;
        }

        public TSignature? Signature { get; set; }

        public abstract LedgerType LedgerType { get; }

        TransactionBase? IPacketBase.Transaction { get => Payload.Transaction; }

        SignatureBase? IPacketBase.Signature { get => Signature; }

        IPayload<TTransaction>? IPacketBase<TTransaction>.Payload => Payload;
    }

    public interface IPacketBase<out TTransaction> : IPacketBase where TTransaction : TransactionBase
    {
        IPayload<TTransaction>? Payload { get; }
    }

    public interface IPacketBase : ISerializableEntity
    {
        LedgerType LedgerType { get; }

        TransactionBase? Transaction { get; }

        SignatureBase? Signature { get; }

        T? AsPacket<T>() where T : class, IPacketBase;

        string ToJson();
    }
}

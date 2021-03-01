using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public abstract class TransactionalTransitionalPacketBase : O10StatePacket
	{

		[JsonConverter(typeof(ByteArrayJsonConverter))]
		/// <summary>
		/// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
		/// </summary>
		public byte[] DestinationKey { get; set; }

		[JsonConverter(typeof(ByteArrayJsonConverter))]
		/// <summary>
		/// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
		/// </summary>
		public byte[] TransactionPublicKey { get; set; }
	}
}

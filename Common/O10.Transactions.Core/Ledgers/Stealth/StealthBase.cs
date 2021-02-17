using O10.Core.Models;

namespace O10.Transactions.Core.Ledgers.Stealth
{
    public abstract class StealthBase : StealthSignedPacketBase
    {
		public override ushort LedgerType => (ushort)Enums.LedgerType.Stealth;

		/// <summary>
		/// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
		/// </summary>
		public byte[] DestinationKey { get; set; }

		/// <summary>
		/// This is destination key of target that sender wants to authorize with
		/// </summary>
		public byte[] DestinationKey2 { get; set; }

		/// <summary>
		/// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
		/// </summary>
		public byte[] TransactionPublicKey { get; set; }
	}
}

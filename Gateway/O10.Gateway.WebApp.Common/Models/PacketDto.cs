namespace O10.Gateway.WebApp.Common.Models
{
	public class PacketDto
	{
        public int TransactionType { get; set; }
        public byte[] ContentTransaction { get; set; }
		public byte[] ContentWitness { get; set; }
	}
}

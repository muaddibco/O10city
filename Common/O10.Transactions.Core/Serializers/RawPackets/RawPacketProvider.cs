using O10.Transactions.Core.Exceptions;
using O10.Core.Architecture;

using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Transactions.Core.Serializers.RawPackets
{

    [RegisterExtension(typeof(IRawPacketProvider), Lifetime = LifetimeManagement.Transient)]
    public class RawPacketProvider : IRawPacketProvider
    {
        private bool _disposed = false; // To detect redundant calls
		private byte[] _content;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IHashCalculation _transactionKeyHashCalculation;
        private readonly IIdentityKeyProvider _transactionKeyIdentityKeyProvider;

        public RawPacketProvider(IRawPacketProvidersFactory rawPacketProvidersFactory, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository)
        {
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _transactionKeyIdentityKeyProvider = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _transactionKeyHashCalculation = hashCalculationsRepository.Create(HashType.MurMur);
        }

        public byte[] GetBytes()
        {
            return _content;
        }

        public void Initialize(IPacket blockBase)
        {
            _disposed = false;

			if (blockBase?.RawData == null)
			{
				throw new RawBytesNotInitializedException();
			}
			_content = blockBase.RawData.ToArray();
		}

		public void Initialize(byte[] content)
		{
			_disposed = false;
			_content = content;
		}

		public void Dispose()
        {
            if (!_disposed)
            {
                _rawPacketProvidersFactory.Utilize(this);

                _disposed = true;
            }
        }

        //TODO: this function is too resource intensive - need to find another way of transaction key obtaining
        public IKey GetKey()
        {
            byte[] hash = _transactionKeyHashCalculation.CalculateHash(_content);
            IKey key = _transactionKeyIdentityKeyProvider.GetKey(hash);

            return key;
        }

	}
}

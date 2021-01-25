using System;
using System.IO;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;


namespace O10.Transactions.Core.Serializers.Signed.Registry
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.Transient)]
    public class RegistryFullBlockSerializer : SignatureSupportSerializerBase<RegistryFullBlock>
    {
        private readonly RegistryRegisterBlockSerializer _transactionRegisterBlockSerializer;
        private readonly RegistryRegisterStealthBlockSerializer _registryRegisterStealthBlockSerializer;

        public RegistryFullBlockSerializer(IServiceProvider serviceProvider) : base(serviceProvider, PacketType.Registry, ActionTypes.Registry_FullBlock)
        {
            _transactionRegisterBlockSerializer = new RegistryRegisterBlockSerializer(serviceProvider);
            _registryRegisterStealthBlockSerializer = new RegistryRegisterStealthBlockSerializer(serviceProvider);
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.StateWitnesses.Length);
            bw.Write((ushort)_block.UtxoWitnesses.Length);

            foreach (var item in _block.StateWitnesses)
            {
                _transactionRegisterBlockSerializer.Initialize(item);
                bw.Write(_transactionRegisterBlockSerializer.GetBytes());
            }

            foreach (var item in _block.UtxoWitnesses)
            {
                _registryRegisterStealthBlockSerializer.Initialize(item);
                bw.Write(_registryRegisterStealthBlockSerializer.GetBytes());
            }

            bw.Write(_block.ShortBlockHash);
        }
    }
}

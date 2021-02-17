using System;
using System.IO;
using O10.Transactions.Core.Enums;
using O10.Core.Models;

namespace O10.Transactions.Core.Serializers.Signed
{
    public abstract class SignatureSupportSerializerBase<T> : SerializerBase<T> where T : SignedPacketBase
    {
        public SignatureSupportSerializerBase(IServiceProvider serviceProvider, LedgerType packetType, ushort blockType)
            : base(serviceProvider, packetType, blockType)
        {
        }

        protected virtual void WriteHeader(BinaryWriter bw)
        {
            bw.Write((ushort)PacketType);
            bw.Write(_block.SyncHeight);
            bw.Write(_block.Nonce);
            bw.Write(_block.PowHash);
        }

        protected abstract void WriteBody(BinaryWriter bw);

        public override void SerializeBody()
        {
            if (_block == null || _serializationBodyDone)
            {
                return;
            }

            FillHeader();

            FillBody();

            _serializationBodyDone = true;
        }

        public override void SerializeFully()
        {
            if (_block == null || _serializationFullyDone)
            {
                return;
            }

            SerializeBody();

            FinalizeTransaction();

            _serializationFullyDone = true;
        }

        #region Private Functions

        private void FillHeader()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            _memoryStream.SetLength(0);

            WriteHeader(_binaryWriter);
        }

        private void FillBody()
        {
            _binaryWriter.Write(_block.Version);
            _binaryWriter.Write(_block.PacketType);
            _binaryWriter.Write(_block.Height);

            WriteBody(_binaryWriter);

            _block.BodyBytes = _memoryStream.ToArray();
        }

        private void FinalizeTransaction()
        {
            _binaryWriter.Write(_block.Signature.ToArray());
            _binaryWriter.Write(_block.Signer.Value.ToArray());
            _block.RawData = _memoryStream.ToArray();
        }
        
        #endregion Private Functions
    }
}

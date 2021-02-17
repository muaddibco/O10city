using System;
using System.Text;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Exceptions;
using O10.Core;
using O10.Core.Architecture;

using O10.Core.Identity;

namespace O10.Transactions.Core.Parsers.Transactional
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class IssueAssetParser : TransactionalBlockParserBase
    {
        public IssueAssetParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => PacketTypes.Transaction_IssueAsset;

        protected override Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase)
        {
            IssueAsset block = null;

            if(version == 1)
            {
                int readBytes = 0;
                byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte strLen = spanBody.Slice(readBytes, 1).ToArray()[0];
                readBytes++;

                string issuedAssetInfo = Encoding.ASCII.GetString(spanBody.Slice(readBytes, strLen).ToArray());
                readBytes += strLen;

                AssetIssuance assetIssuance = new AssetIssuance
                {
                    AssetId = assetId,
                    IssuedAssetInfo = issuedAssetInfo
                };

                block = new IssueAsset
                {
                    AssetIssuance = assetIssuance
                };

                transactionalBlockBase = block;
                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}

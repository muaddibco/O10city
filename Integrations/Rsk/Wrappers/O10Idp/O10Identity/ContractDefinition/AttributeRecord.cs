using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace O10Idp.Contracts.O10Identity.ContractDefinition
{
    public partial class AttributeRecord : AttributeRecordBase { }

    public class AttributeRecordBase 
    {
        [Parameter("bytes32", "BindingCommitment", 1)]
        public virtual byte[] BindingCommitment { get; set; }
        [Parameter("bytes32", "AssetCommitment", 2)]
        public virtual byte[] AssetCommitment { get; set; }
        [Parameter("string", "AttributeName", 3)]
        public virtual string AttributeName { get; set; }
        [Parameter("int256", "Version", 4)]
        public virtual BigInteger Version { get; set; }
        [Parameter("uint256", "AttributeId", 5)]
        public virtual BigInteger AttributeId { get; set; }
    }
}

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace O10Idp.Contracts.O10Identity.ContractDefinition
{
    public partial class IssuerDetails : IssuerDetailsBase { }

    public class IssuerDetailsBase 
    {
        [Parameter("address", "Address", 1)]
        public virtual string Address { get; set; }
        [Parameter("string", "Alias", 2)]
        public virtual string Alias { get; set; }
    }
}

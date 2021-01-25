using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;

namespace O10Idp.Contracts.O10Identity.ContractDefinition
{
    public partial class AttributeDefinition : AttributeDefinitionBase { }

    public class AttributeDefinitionBase 
    {
        [Parameter("string", "AttributeName", 1)]
        public virtual string AttributeName { get; set; }
        [Parameter("string", "AttributeScheme", 2)]
        public virtual string AttributeScheme { get; set; }
        [Parameter("string", "Alias", 3)]
        public virtual string Alias { get; set; }
        [Parameter("bool", "IsRoot", 4)]
        public virtual bool IsRoot { get; set; }
    }
}

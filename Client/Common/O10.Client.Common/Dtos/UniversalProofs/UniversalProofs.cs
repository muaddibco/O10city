#nullable enable
using Newtonsoft.Json;
using System.Collections.Generic;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Client.Common.Dtos.UniversalProofs
{
    public class UniversalProofs
    {
        public UniversalProofs()
        {
            RootIssuers = new List<RootIssuer>();
        }

        public UniversalProofsMission Mission { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? KeyImage { get; set; }

        public string? SessionKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? MainIssuer { get; set; }

        public List<RootIssuer> RootIssuers { get; set; }

        public object? Payload { get; set; }
    }

    public class RootIssuer
    {
        public RootIssuer()
        {
            IssuersAttributes = new List<AttributesByIssuer>();
        }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? Issuer { get; set; }

        public List<AttributesByIssuer>? IssuersAttributes { get; set; }
    }

    public class CommitmentProof
    {
        public List<string>? Values { get; set; }

        /// <summary>
        /// If Values is not null or empty then it is Issuance Proof, otherwise it is SurjectionProof of main Commitment (e.g. Authentication Proof)
        /// </summary>
        public SurjectionProof? SurjectionProof { get; set; }
    }

    public class AttributeProofs
    {
        /// <summary>
        /// If it is proofs of a root attribute of a Root Identity then this property holds a commitment created as follows:
        /// Ca = rx*G + Ir
        /// In the case it is a commitment to associated attribute value at IdP is calculated as follows:
        /// Ca = r1*G + Ia + Ir
        /// 
        /// </summary>
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? Commitment { get; set; }

        public string? SchemeName { get; set; }

        public SurjectionProof? BindingProof { get; set; }

        /// <summary>
        /// For revealing real value or proving registrations
        /// </summary>
        public CommitmentProof? CommitmentProof { get; set; }
    }

    public class AttributesByIssuer
    {
        public AttributesByIssuer()
        {
            Attributes = new List<AttributeProofs>();
        }

        public AttributesByIssuer(IKey issuer)
        {
            Issuer = issuer;
            Attributes = new List<AttributeProofs>();
        }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? Issuer { get; set; }

        public List<AttributeProofs>? Attributes { get; set; }

        public AttributeProofs? RootAttribute { get; set; }
    }
}
﻿using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Crypto.Models
{
    /// <summary>
    /// data for passing the asset to the receiver secretly
    /// If the pedersen commitment to an asset is C = aG + I,
    /// "Mask" contains a 32 byte key 'a'; Asset can be calculated from Commitment
    /// </summary>
    public class EcdhTupleCA
    {
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] Mask { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] AssetId { get; set; }
    }
}
